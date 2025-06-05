namespace Reactive.Core.Utils;

/// <summary>
/// Provides static methods to manage the tracking and untracking of reactive effects.
/// </summary>
public static class Tracker
{
    /// <summary>
    /// Tracks the execution of a code block within the context of a specified effect.
    /// Sets the current effect, executes the body, and restores the previous effect.
    /// </summary>
    /// <param name="effect">The effect to track during execution.</param>
    /// <param name="body">The code block to execute while tracking.</param>
    public static void Track(Effect effect, Action body)
    {
        var prev = Effect.Current;
        Effect.Current = effect;

        try
        {
            body();
        }
        finally
        {
            Effect.Current = prev;
        }
    }

    /// <summary>
    /// Executes a function without tracking any effect dependencies.
    /// Temporarily clears the current effect, runs the function, and restores the previous effect.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="fn">The function to execute untracked.</param>
    /// <returns>The result of the function.</returns>
    public static T Untracked<T>(Func<T> fn)
    {
        var prev = Effect.Current;
        Effect.Current = null;

        try
        {
            return fn();
        }
        finally
        {
            Effect.Current = prev;
        }
    }

    /// <summary>
    /// Executes an action without tracking any effect dependencies.
    /// Temporarily clears the current effect, runs the action, and restores the previous effect.
    /// </summary>
    /// <param name="action">The action to execute untracked.</param>
    public static void Untracked(Action action)
    {
        var prev = Effect.Current;
        Effect.Current = null;

        try
        {
            action();
        }
        finally
        {
            Effect.Current = prev;
        }
    }
}