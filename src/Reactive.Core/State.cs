using Reactive.Core.Interfaces;
using static Reactive.Core.Utils.Scheduler;

namespace Reactive.Core;

public class State<TValue>(TValue initialValue) : IWritableState<TValue>
{
    private readonly HashSet<Effect> _subscribers = [];

    private bool _disposed;
    private TValue _value = initialValue;

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public TValue Get()
    {
        if (Effect.Current is { } effect)
        {
            _subscribers.Add(effect);
        }

        return _value;
    }

    void IState.Link(Effect effect)
    {
        _subscribers.Add(effect);
    }

    public void Set(TValue value)
    {
        if (EqualityComparer<TValue>.Default.Equals(_value, value))
        {
            return;
        }

        _value = value;

        foreach (var subscriber in _subscribers.ToList())
        {
            Schedule(subscriber.Execute);
        }
    }

    public void Set(Func<TValue, TValue> updater)
    {
        Set(updater(_value));
    }

    void IState.Unlink(Effect effect)
    {
        _subscribers.Remove(effect);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _subscribers.Clear();
            }

            _disposed = true;
        }
    }
}