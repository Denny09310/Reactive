using Reactive.Core.Interfaces;
using static Reactive.Core.Utils.Tracker;

namespace Reactive.Core;

public class Effect : IDisposable
{
    [ThreadStatic]
    internal static Effect? Current;

    private readonly Func<Action?> _callback;
    private readonly HashSet<IState> _dependencies = [];

    private Action? _cleanup;
    private bool _disposed;

    public Effect(Func<Action?> callback)
    {
        _callback = callback;
        Execute();
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public void Execute()
    {
        Invalidate();
        Track(this, () =>
        {
            _cleanup = _callback();
        });
    }

    public void Link(IState signal)
    {
        _dependencies.Add(signal);
        signal.Link(this);
    }

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