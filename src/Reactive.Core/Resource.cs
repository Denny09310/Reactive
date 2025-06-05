using Reactive.Core.Interfaces;
using System.Diagnostics.CodeAnalysis;
using static Reactive.Core.Extensions.Reactivity;
using static Reactive.Core.Utils.Scheduler;

namespace Reactive.Core;

/// <summary>
/// Represents the possible states of a resource.
/// </summary>
public enum ResourceState
{
    Idle,
    Loading,     // Initial load, no prior data
    Success,     // Data loaded, not refreshing
    Refreshing,  // Data loaded, refresh in progress
    Error,
}

/// <summary>
/// Resource with a parameterless loader. Use <see cref="Refetch"/> to trigger loading.
/// </summary>
public class Resource<T> : ResourceBase<T>
{
    private readonly Func<CancellationToken, Task<T>> _loader;

    /// <summary>
    /// Initializes a new instance of the <see cref="Resource{T}"/> class.
    /// </summary>
    /// <param name="loader">The function to load the resource.</param>
    /// <param name="start">If true, starts loading immediately.</param>
    public Resource(Func<CancellationToken, Task<T>> loader, bool start = true)
    {
        _loader = loader;

        if (start)
        {
            _ = Refetch();
        }
    }

    /// <summary>
    /// Triggers a data fetch using the loader.
    /// </summary>
    public Task Refetch()
    {
        return Refetch(_loader);
    }
}

/// <summary>
/// Base class for resources, handling state, cancellation, and fetch logic.
/// </summary>
public abstract class ResourceBase<TValue> : IDisposable
{
    #region States

    protected readonly State<Exception?> _error = new(null);
    protected readonly State<ResourceState> _status = new(ResourceState.Idle);
    protected readonly State<TValue?> _value = new(default);

    #endregion States

    private CancellationTokenSource? _cts;
    private bool _disposed;

    /// <summary>
    /// True if the resource has a value (Success or Refreshing).
    /// </summary>
    [MemberNotNullWhen(false, nameof(Error))]
    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue => Status.Get() is ResourceState state &&
        (state == ResourceState.Success || state == ResourceState.Refreshing);

    /// <summary>
    /// True if the resource is loading for the first time.
    /// </summary>
    public bool IsLoading => Status.Get() == ResourceState.Loading;

    /// <summary>
    /// True if a refresh is in progress and a value is available.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsRefreshing => Status.Get() == ResourceState.Refreshing;

    #region Signals

    /// <summary>
    /// The current error, if any.
    /// </summary>
    public IState<Exception?> Error => _error;

    /// <summary>
    /// The current status of the resource.
    /// </summary>
    public IState<ResourceState> Status => _status;

    /// <summary>
    /// The current value of the resource.
    /// </summary>
    public IState<TValue?> Value => _value;

    #endregion Signals

    /// <summary>
    /// Disposes the resource and cancels any in-flight operations.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes managed and unmanaged resources.
    /// </summary>
    /// <param name="disposing">True if called from Dispose.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

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

    /// <summary>
    /// Cancels any in-flight fetch and starts a new one, handling state transitions and errors.
    /// </summary>
    /// <param name="loader">The function to load the resource.</param>
    protected async Task Refetch(Func<CancellationToken, Task<TValue>> loader)
    {
        if (_cts is not null)
        {
            await _cts.CancelAsync();
            _cts.Dispose();
        }

        _cts = new CancellationTokenSource();

        var token = _cts.Token;

        var success = _status.Get() == ResourceState.Success;
        if (success)
        {
            _status.Set(ResourceState.Refreshing);
        }
        else
        {
            Batch(() =>
            {
                _status.Set(ResourceState.Loading);
                _value.Set(default);
                _error.Set(default);
            });
        }

        try
        {
            var result = await loader(token);

            if (token.IsCancellationRequested) return;

            Batch(() =>
            {
                _error.Set(default);
                _value.Set(result);
                _status.Set(ResourceState.Success);
            });
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
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
            if (token.IsCancellationRequested) return;

            Batch(() =>
            {
                _error.Set(ex);
                _status.Set(ResourceState.Error);
            });
        }
    }
}

/// <summary>
/// Resource with reactive arguments. Automatically refetches when arguments change.
/// </summary>
public class Resource<TArgs, TValue> : ResourceBase<TValue>
{
    private readonly Computed<TArgs> _args;
    private readonly Effect _effect;
    private readonly Func<TArgs, CancellationToken, Task<TValue>> _loader;

    /// <summary>
    /// Initializes a new instance of the <see cref="Resource{TArgs, TValue}"/> class.
    /// </summary>
    /// <param name="args">Function to get the current arguments.</param>
    /// <param name="loader">Function to load the resource with arguments.</param>
    /// <param name="start">If true, starts loading immediately.</param>
    public Resource(Func<TArgs> args, Func<TArgs, CancellationToken, Task<TValue>> loader, bool start = true)
    {
        _args = Signal.Computed(args);
        _loader = loader;

        var initial = _args.Get();
        TArgs last = initial;

        if (start)
        {
            _ = Refetch(initial);
        }

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
    /// Gets the current arguments as a computed state.
    /// </summary>
    public IState<TArgs> Args => _args;

    /// <summary>
    /// Triggers a fetch with the specified arguments.
    /// </summary>
    public Task Refetch(TArgs args) => Refetch(ct => _loader(args, ct));

    /// <inheritdoc/>
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