namespace Reactive.Core;

/// <summary>
/// Provides static factory methods for creating reactive signals, such as computed and state values.
/// </summary>
public static class Signal
{
    /// <summary>
    /// Creates a computed reactive value that automatically updates when its dependencies change.
    /// </summary>
    /// <typeparam name="T">The type of the computed value.</typeparam>
    /// <param name="compute">A function that computes the value based on dependencies.</param>
    /// <returns>A <see cref="Computed{T}"/> instance representing the computed value.</returns>
    public static Computed<T> Computed<T>(Func<T> compute) => new(compute);

    /// <summary>
    /// Creates a reactive state value that can be read and updated.
    /// </summary>
    /// <typeparam name="T">The type of the state value.</typeparam>
    /// <param name="initialValue">The initial value of the state.</param>
    /// <returns>A <see cref="State{T}"/> instance representing the state value.</returns>
    public static State<T> State<T>(T initialValue) => new(initialValue);
}