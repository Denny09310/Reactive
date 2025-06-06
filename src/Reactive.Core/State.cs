using Reactive.Core.Interfaces;
using static Reactive.Core.Utils.Scheduler;

namespace Reactive.Core;

/// <summary>
/// Represents a reactive state container that holds a value and notifies subscribers (effects) when the value changes.
/// </summary>
/// <typeparam name="TValue">The type of the value stored in the state.</typeparam>
public class State<TValue>(TValue initialValue) : IWritableState<TValue>
{
    /// <summary>
    /// The set of effects subscribed to this state.
    /// </summary>
    private readonly HashSet<Effect> _subscribers = [];

    /// <summary>
    /// Indicates whether the state has been disposed.
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// The current value of the state.
    /// </summary>
    private TValue _value = initialValue;

    /// <summary>
    /// Disposes the state, clearing all subscribers and suppressing finalization.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Gets the current value of the state. If called within an effect, subscribes the effect to this state.
    /// </summary>
    /// <returns>The current value.</returns>
    public TValue Get()
    {
        if (Effect.Current is { } effect)
        {
            effect.Link(this);
        }

        return _value;
    }

    /// <summary>
    /// Sets the value of the state. Notifies all subscribers if the value changes.
    /// </summary>
    /// <param name="value">The new value to set.</param>
    public void Set(TValue value)
    {
        if (EqualityComparer<TValue>.Default.Equals(_value, value))
        {
            return;
        }

        _value = value;

        Batch(() =>
        {
            foreach (var subscriber in _subscribers.ToList())
            {
                Schedule(subscriber);
            }
        });
    }

    /// <summary>
    /// Updates the value of the state using an updater function. Notifies all subscribers if the value changes.
    /// </summary>
    /// <param name="updater">A function that takes the current value and returns the new value.</param>
    public void Update(Func<TValue, TValue> updater)
    {
        Set(updater(_value));
    }

    /// <summary>
    /// Links an effect to this state, subscribing it to value changes.
    /// </summary>
    /// <param name="effect">The effect to link.</param>
    void IState.Link(Effect effect)
    {
        _subscribers.Add(effect);
    }

    /// <summary>
    /// Unlinks an effect from this state, unsubscribing it from value changes.
    /// </summary>
    /// <param name="effect">The effect to unlink.</param>
    void IState.Unlink(Effect effect)
    {
        _subscribers.Remove(effect);
    }

    /// <summary>
    /// Disposes the state, clearing all subscribers if disposing is true.
    /// </summary>
    /// <param name="disposing">True if called from Dispose; false if called from finalizer.</param>
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