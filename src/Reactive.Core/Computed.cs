using Reactive.Core.Interfaces;
using static Reactive.Core.Extensions.Reactivity;
using static Reactive.Core.Utils.Scheduler;

namespace Reactive.Core;

/// <summary>
/// Represents a computed reactive state that automatically updates its value
/// when its dependencies change, and notifies subscribers of changes.
/// </summary>
/// <typeparam name="T">The type of the computed value.</typeparam>
public class Computed<T> : IState<T>
{
    /// <summary>
    /// The function used to compute the value.
    /// </summary>
    private readonly Func<T> _compute;

    /// <summary>
    /// The effect that tracks dependencies and triggers recomputation.
    /// </summary>
    private readonly Effect _effect;

    /// <summary>
    /// The set of effects that subscribe to changes in this computed value.
    /// </summary>
    private readonly HashSet<Effect> _subscribers = [];

    private bool _disposed;

    /// <summary>
    /// The current value of the computed state.
    /// </summary>
    private T _value = default!;

    /// <summary>
    /// Initializes a new instance of the <see cref="Computed{T}"/> class.
    /// </summary>
    /// <param name="compute">The function to compute the value.</param>
    public Computed(Func<T> compute)
    {
        _compute = compute;

        _effect = Effect(() =>
        {
            var result = _compute();
            if (EqualityComparer<T>.Default.Equals(_value, result))
            {
                return;
            }

            _value = result;

            Effect[] subscribers = [.. _subscribers];
            _subscribers.Clear();

            Batch(() =>
            {
                foreach (var subscriber in subscribers)
                {
                    Schedule(subscriber);
                }
            });
        });
    }

    /// <summary>
    /// Disposes the computed state and releases all resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Gets the current value of the computed state and links the current effect as a subscriber.
    /// </summary>
    /// <returns>The current computed value.</returns>
    public T Get()
    {
        if (Effect.Current is { } effect)
        {
            effect.Link(this);
        }

        return _value;
    }

    /// <summary>
    /// Adds an effect as a subscriber to this computed state.
    /// </summary>
    /// <param name="effect">The effect to link.</param>
    void IState.Link(Effect effect)
    {
        _subscribers.Add(effect);
    }

    /// <summary>
    /// Removes an effect from the subscribers of this computed state.
    /// </summary>
    /// <param name="effect">The effect to unlink.</param>
    void IState.Unlink(Effect effect)
    {
        _subscribers.Remove(effect);
    }

    /// <summary>
    /// Disposes the computed state, optionally releasing managed resources.
    /// </summary>
    /// <param name="disposing">True to release managed resources; otherwise, false.</param>
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