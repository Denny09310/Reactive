using Reactive.Core.Interfaces;
using static Reactive.Core.Extensions.Reactivity;

namespace Reactive.Core;

public class Computed<T> : IState<T>
{
    private readonly Func<T> _compute;
    private readonly Effect _effect;
    private readonly HashSet<Effect> _subscribers = [];

    private bool _disposed;
    private T _value = default!;

    public Computed(Func<T> compute)
    {
        _compute = compute;
        _effect = Effect(() =>
        {
            var result = _compute();
            if (!EqualityComparer<T>.Default.Equals(_value, result))
            {
                _value = result;

                foreach (var subscriber in _subscribers.ToList())
                {
                    subscriber.Execute();
                }
            }
        });
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public T Get()
    {
        if (Effect.Current is { } effect)
        {
            effect.Link(this);
        }

        return _value;
    }

    void IState.Link(Effect effect)
    {
        _subscribers.Add(effect);
    }

    void IState.Unlink(Effect effect)
    {
        _subscribers.Remove(effect);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _effect.Dispose();
            _subscribers.Clear();
        }

        _disposed = true;
    }
}