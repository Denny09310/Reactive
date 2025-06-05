namespace Reactive.Core.Utils
{
    /// <summary>
    /// Provides batching and scheduling utilities for executing actions (effects),
    /// but tracks nested batches with a depth counter instead of a simple boolean.
    /// </summary>
    public static class Scheduler
    {
        // Holds all effects that need to run once the outermost batch completes.
        private static readonly HashSet<Effect> _effects = [];

        // Instead of a bool, we track how many Batch(...) calls are currently "open."
        private static int _depth = 0;

        /// <summary>
        /// Executes the given action within a batching context.
        /// Nested Batch(...) invocations simply increment the counter and do not flush effects
        /// until the outermost Batch is closed.
        /// </summary>
        public static void Batch(Action action)
        {
            // Enter a new batch scope
            _depth++;

            try
            {
                action();
            }
            finally
            {
                // Exit this batch scope
                _depth--;

                // Only the *outermost* Batch call actually flushes & executes
                if (_depth == 0)
                {
                    // Drain the set of scheduled effects exactly once,
                    // in case the action (or nested batches) added more.
                    Effect[] effects = [.. _effects];
                    _effects.Clear();

                    foreach (var effect in effects)
                    {
                        effect.Execute();
                    }
                }
            }
        }

        /// <summary>
        /// Schedules an effect to run. If we're inside *any* batch (depth > 0),
        /// just enqueue it; otherwise, run it immediately.
        /// </summary>
        public static void Schedule(Effect effect)
        {
            if (_depth > 0)
            {
                _effects.Add(effect);
            }
            else
            {
                effect.Execute();
            }
        }
    }
}