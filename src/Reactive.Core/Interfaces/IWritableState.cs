namespace Reactive.Core.Interfaces;

/// <summary>
/// Represents a writable reactive state that allows both reading and updating its value.
/// Inherits from <see cref="IState{TValue}"/> to provide read access, and adds methods for setting the value.
/// </summary>
/// <typeparam name="TValue">The type of the value stored in the state.</typeparam>
public interface IWritableState<TValue> : IState<TValue>
{
    /// <summary>
    /// Sets the state to the specified value.
    /// </summary>
    /// <param name="value">The new value to set.</param>
    void Set(TValue value);

    /// <summary>
    /// Updates the state using the provided updater function, which receives the current value and returns the new value.
    /// </summary>
    /// <param name="updater">A function that takes the current value and returns the updated value.</param>
    void Set(Func<TValue, TValue> updater);
}