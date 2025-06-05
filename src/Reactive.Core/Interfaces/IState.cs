namespace Reactive.Core.Interfaces;

/// <summary>
/// Represents a reactive state that can be linked to one or more <see cref="Effect"/> instances.
/// Provides methods for managing effect dependencies and resource cleanup.
/// </summary>
public interface IState : IDisposable
{
    /// <summary>
    /// Links the specified <see cref="Effect"/> to this state, so that the effect is notified when the state changes.
    /// </summary>
    /// <param name="effect">The effect to link.</param>
    internal abstract void Link(Effect effect);

    /// <summary>
    /// Unlinks the specified <see cref="Effect"/> from this state, so that the effect is no longer notified of changes.
    /// </summary>
    /// <param name="effect">The effect to unlink.</param>
    internal abstract void Unlink(Effect effect);
}

/// <summary>
/// Represents a typed reactive state that can provide its current value.
/// </summary>
/// <typeparam name="TValue">The type of the value held by the state.</typeparam>
public interface IState<out TValue> : IState
{
    /// <summary>
    /// Gets the current value of the state.
    /// </summary>
    /// <returns>The current value.</returns>
    TValue Get();
}