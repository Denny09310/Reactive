using Reactive.Core.Interfaces;
using static Reactive.Core.Utils.Tracker;

namespace Reactive.Core;

/// <summary>
/// Represents a reactive effect that tracks dependencies and executes a callback when dependencies change.
/// </summary>
public class Effect : IDisposable
{
    /// <summary>
    /// Holds the current effect being tracked in the current thread.
    /// </summary>
    [ThreadStatic]
    internal static Effect? Current;

    /// <summary>
    /// The callback function to execute when dependencies change. Returns an optional cleanup action.
    /// </summary>
    private readonly Func<Action?> _callback;

    /// <summary>
    /// The set of state dependencies this effect is linked to.
    /// </summary>
    private readonly HashSet<IState> _dependencies = [];

    /// <summary>
    /// The cleanup action to run before re-executing or disposing the effect.
    /// </summary>
    private Action? _cleanup;

    /// <summary>
    /// Indicates whether the effect has been disposed.
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="Effect"/> class and executes the callback.
    /// </summary>
    /// <param name="callback">The callback to execute when dependencies change.</param>
    public Effect(Func<Action?> callback)
    {
        _callback = callback;
        Execute();
    }

    /// <summary>
    /// Indicates whether this effect has already been scheduled during the current batch.
    /// Used to prevent duplicate executions.
    /// </summary>
    internal bool Scheduled { get; set; } = false;

    /// <summary>
    /// Disposes the effect and releases all dependencies.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Executes the effect, tracking dependencies and running the callback.
    /// </summary>
    public void Execute()
    {
        Scheduled = false;

        Invalidate();
        Track(this, () =>
        {
            _cleanup = _callback();
        });
    }

    /// <summary>
    /// Links this effect to a state dependency.
    /// </summary>
    /// <param name="signal">The state to link.</param>
    public void Link(IState signal)
    {
        if (!_dependencies.Add(signal))
        {
            return;
        }

        signal.Link(this);
    }

    /// <summary>
    /// Disposes the effect, optionally releasing managed resources.
    /// </summary>
    /// <param name="disposing">True if called from Dispose; false if called from finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            Invalidate();
        }

        _disposed = true;
    }

    /// <summary>
    /// Invalidates the effect, unlinking all dependencies and running cleanup.
    /// </summary>
    private void Invalidate()
    {
        foreach (var dependency in _dependencies.ToList())
        {
            dependency.Unlink(this);
        }

        _dependencies.Clear();

        _cleanup?.Invoke();
        _cleanup = null;
    }
}