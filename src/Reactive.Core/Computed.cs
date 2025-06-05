using Reactive.Core.Interfaces;

namespace Reactive.Core;

public class Computed<T> : IState<T>
{
    private readonly Func<T> _compute;
    private readonly HashSet<Effect> _subscribers = [];

    private T _value = default!;

    public Computed(Func<T> compute)
    {
        _compute = compute;
        var effect = new Effect(() =>
        {
            var result = _compute();
            if (!EqualityComparer<T>.Default.Equals(_value, result))
            {
                _value = result;
                NotifySubscribers();
            }
        });
    }

    public T Get()
    {
        if (Effect.Current is { } effect)
        {
            effect.Register(this);
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

    private void NotifySubscribers()
    {
        foreach (var subscriber in _subscribers.ToList())
        {
            subscriber.Invalidate();
        }
    }
}