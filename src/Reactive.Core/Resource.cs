using Reactive.Core.Interfaces;
using System.Diagnostics.CodeAnalysis;
using static Reactive.Core.Extensions.Reactivity;
using static Reactive.Core.Utils.Scheduler;
using static Reactive.Core.Utils.Tracker;

namespace Reactive.Core;

/// <summary>
/// Shared enum for all Resource states.
/// </summary>
public enum ResourceState
{
    Idle,
    Loading,     // initial load: no prior data
    Success,     // data loaded, not refreshing
    Refreshing,  // data loaded, but a new fetch is in progress
    Error,
}

/// <summary>
/// “Parameterless” Resource: loader is just Func<CancellationToken, Task<T>>,
/// and Refetch() always calls Refresh(_loader).
/// </summary>
public class Resource<T> : ResourceBase<T>
{
    private readonly Func<CancellationToken, Task<T>> _loader;

    public Resource(Func<CancellationToken, Task<T>> loader, bool start = true)
    {
        _loader = loader;

        if (start)
        {
            // Fire‐and‐forget the first load.
            _ = Refetch();
        }
    }

    /// <summary>
    /// Kick off (or re‐kick) a data fetch.
    /// Internally, calls <see cref="ResourceBase{TValue}.Refetch(Func{CancellationToken, Task{TValue}})"/> which handles cancellation + stale revalidate.
    /// </summary>
    public Task Refetch()
    {
        return Refetch(_loader);
    }
}

/// <summary>
/// Abstract base that holds all the common fields and the “fetch‐with‐cancellation + stale-while-revalidate” logic.
/// Subclasses merely supply a Func<CancellationToken, Task<TValue>> (possibly capturing arguments).
/// </summary>
public abstract class ResourceBase<TValue> : IDisposable
{
    //———— Common backing fields ————
    // These three States are exactly the same in both variants:

    #region States

    protected readonly State<Exception?> _error = new(null);
    protected readonly State<ResourceState> _status = new(ResourceState.Idle);
    protected readonly State<TValue?> _value = new(default);

    #endregion States

    private CancellationTokenSource? _cts;
    private bool _disposed;

    //———— Public API (exposed to consumers) ————

    /// <summary>
    /// True if (Status == Success or Status == Refreshing).
    /// During “Refreshing” we keep showing the old _value until the new fetch finishes.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Error))]
    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue => Status.Get() is ResourceState state &&
        state == ResourceState.Success ||
        state == ResourceState.Refreshing;

    /// <summary>
    /// True only if this is the very first load (Status == Loading and no prior data).
    /// </summary>
    public bool IsLoading =>
        Status.Get() == ResourceState.Loading;

    /// <summary>
    /// True if we already had a successful value and have kicked off a new fetch (Status == Refreshing).
    /// Consumer can choose to keep displaying the old Value while a refresh is in flight.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsRefreshing =>
        Status.Get() == ResourceState.Refreshing;

    #region Signals

    public IState<Exception?> Error => _error;

    public IState<ResourceState> Status => _status;

    public IState<TValue?> Value => _value;

    #endregion Signals

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _cts?.Cancel();
            _cts?.Dispose();

            _status.Dispose();
            _value.Dispose();
            _error.Dispose();
        }

        _cts = null;
        _disposed = true;
    }

    //———— Core logic: “cancel old fetch + run new fetch with stale-while-revalidate” ————
    //
    // Any subclass will call this, passing in a Func<CancellationToken, Task<TValue>>
    // that actually does the data loading.  We handle:
    //   • canceling the previous CTS
    //   • deciding whether to go into Loading (if no prior success) or Refreshing (if we already had data)
    //   • invoking the loader, catching OperationCanceledException vs. other Exception
    //   • updating _value, _error, and _status accordingly
    //
    // “loader” can capture any external argument (e.g. TArgs) if needed.
    protected async Task Refetch(Func<CancellationToken, Task<TValue>> loader)
    {
        // 1) Cancel the previous in‐flight request (if any)
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        var token = _cts.Token;

        // 2) If we already succeeded once, go into Refreshing (keep old _value).
        //    Otherwise (Idle or Error), go into Loading, clear old _value and old _error.
        var success = _status.Get() == ResourceState.Success;
        if (success)
        {
            _status.Set(ResourceState.Refreshing);
            // note: do NOT clear _value here, so consumers still see the old Value.
            // _error stays as whatever it was (likely null).
        }
        else
        {
            Batch(() =>
            {
                _status.Set(ResourceState.Loading);
                _value.Set(default(TValue)); // explicitly clear old data
                _error.Set(default(Exception));    // clear previous error
            });
        }

        try
        {
            // 3) Actually perform the loader.  If it throws OperationCanceledException
            //    (and token was canceled), we swallow it.  Otherwise catch any other Exception.
            var result = await loader(token);

            // If cancellation was requested mid‐await, bail out and do NOT touch _value/_status.
            if (token.IsCancellationRequested) return;

            // 4) Success: set new value + status
            Batch(() =>
            {
                _error.Set(default(Exception));
                _value.Set(result);
                _status.Set(ResourceState.Success);
            });
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            // 5a) Canceled before completion: revert to Success or Idle
            if (success)
            {
                _status.Set(ResourceState.Success);
            }
            else
            {
                _status.Set(ResourceState.Idle);
            }
        }
        catch (Exception ex)
        {
            // 5b) If cancellation already triggered, bail out.
            if (token.IsCancellationRequested) return;

            // Otherwise, record the exception & set status=Error.
            Batch(() =>
            {
                _error.Set(ex);
                _status.Set(ResourceState.Error);
            });
        }
    }
}

/// <summary>
/// “Reactive-argument” Resource: whenever the upstream Computed<TArgs> changes,
/// automatically re-fetch with the new argument.  Exposes Args as a Computed signal.
/// </summary>
public class Resource<TArgs, TValue> : ResourceBase<TValue>
{
    private readonly Computed<TArgs> _args;
    private readonly Effect _effect;
    private readonly Func<TArgs, CancellationToken, Task<TValue>> _loader;

    /// <summary>
    /// argsFactory: a function that returns the current TArgs.
    /// loader: a function (TArgs, CancellationToken) → Task<TValue>.
    /// If start==true, does one initial fetch using argsFactory(); otherwise waits.
    /// </summary>
    public Resource(Func<TArgs> args, Func<TArgs, CancellationToken, Task<TValue>> loader, bool start = true)
    {
        // 1) Wrap the argsFactory in a Computed<TArgs>, so we get a reactive signal for arguments
        _args = Signal.Computed(args);
        _loader = loader;

        // 2) Capture the very first argument so that the first Effect run does not re-fetch
        var initial = _args.Get();
        TArgs last = initial;

        // 3) If start==true, do exactly one initial fetch
        if (start)
        {
            _ = Refetch(initial);
        }

        // 4) Create an Effect that only fires when _args.Get() changes from last
        _effect = Effect(() =>
        {
            var current = _args.Get();
            if (EqualityComparer<TArgs>.Default.Equals(current, last))
                return;

            last = current;
            _ = Refetch(current);
        });
    }

    /// <summary>
    /// Publicly expose the current args (untracked).
    /// Consumers can read or compute off this if needed.
    /// </summary>
    /// <remarks>
    /// If you need to access the <see cref="Args"/> inside an <see cref="Effect"/>
    /// wrap the call with <see cref="Untracked{T}(Func{T})"/> to avoid duplicate fire-and-forget
    /// </remarks>
    public IState<TArgs> Args => _args;

    /// <summary>
    /// Manually trigger a fetch using a specific argument.
    /// </summary>
    public Task Refetch(TArgs args) => Refetch(ct => _loader(args, ct));

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _effect.Dispose();
            _args.Dispose();
        }

        base.Dispose(disposing);
    }
}