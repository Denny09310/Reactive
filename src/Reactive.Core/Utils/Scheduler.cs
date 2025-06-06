namespace Reactive.Core.Utils;

/// <summary>
/// Provides batching and scheduling utilities for executing reactive effects.
/// Uses a depth counter to support nested batching, and ensures that each effect
/// is only scheduled and executed once per batch.
/// </summary>
public static class Scheduler
{
    /// <summary>
    /// Holds the set of effects scheduled to run at the end of the batch.
    /// Duplicate entries are avoided via HashSet semantics.
    /// </summary>
    private static readonly HashSet<Effect?> _schedule = [];

    /// <summary>
    /// Tracks nested batching depth. Only the outermost batch will flush the effect queue.
    /// </summary>
    private static int _depth = 0;

    /// <summary>
    /// Executes the given action inside a reactive batching context.
    /// Nested batches do not flush scheduled effects until the outermost batch completes.
    /// </summary>
    /// <param name="action">The action to perform inside the batch.</param>
    public static void Batch(Action action)
    {
        _depth++;

        try
        {
            action();
        }
        finally
        {
            _depth--;

            if (_depth == 0)
            {
                FlushScheduling();
            }
        }
    }

    /// <summary>
    /// Schedules an effect to run. If currently batching, queues it;
    /// otherwise, runs it immediately. Ensures each effect is only run once per batch.
    /// </summary>
    /// <param name="effect">The effect to schedule.</param>
    public static void Schedule(Effect effect)
    {
        if (effect.Scheduled)
            return;

        effect.Scheduled = true;

        if (_depth > 0)
        {
            _schedule.Add(effect);
        }
        else
        {
            effect.Execute();
            effect.Scheduled = false;
        }
    }

    private static void FlushScheduling()
    {
        // Drain effects while allowing new ones to be scheduled during execution
        while (_schedule.Count > 0)
        {
            var effects = _schedule.ToArray();
            _schedule.Clear();

            foreach (var effect in effects)
            {
                effect?.Execute();
            }
        }
    }
}