namespace Reactive.Core.Utils;

/// <summary>
/// Provides batching and scheduling utilities for executing actions (effects).
/// </summary>
public static class Scheduler
{
    /// <summary>
    /// Stores scheduled effects to be executed after batching.
    /// </summary>
    private static readonly HashSet<Action> _effects = [];

    /// <summary>
    /// Indicates whether batching is currently active.
    /// </summary>
    private static bool _batching = false;

    /// <summary>
    /// Executes the given action within a batching context.
    /// All effects scheduled during the action are collected and executed after the batch completes.
    /// </summary>
    /// <param name="action">The action to execute within the batch.</param>
    public static void Batch(Action action)
    {
        _batching = true;
        action();
        _batching = false;

        foreach (var effect in _effects.ToList())
        {
            effect();
        }

        _effects.Clear();
    }

    /// <summary>
    /// Schedules an effect to be executed.
    /// If batching is active, the effect is queued; otherwise, it is executed immediately.
    /// </summary>
    /// <param name="effect">The effect to schedule.</param>
    public static void Schedule(Action effect)
    {
        if (_batching)
        {
            _effects.Add(effect);
        }
        else
        {
            effect();
        }
    }
}