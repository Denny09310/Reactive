namespace Reactive.Core.Utils;

public static class Scheduler
{
    private static readonly HashSet<Action> _effects = [];
    private static readonly Dictionary<string, CancellationTokenSource> _tokens = new();
    private static bool _batching = false;

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

    public static void Debounce(string key, TimeSpan delay, Action action)
    {
        if (_tokens.TryGetValue(key, out var existing))
        {
            existing.Cancel();
            existing.Dispose();
        }

        var cts = new CancellationTokenSource();
        _tokens[key] = cts;

        Task.Delay(delay, cts.Token).ContinueWith(task =>
        {
            if (!task.IsCanceled)
            {
                action();
                _tokens.Remove(key);
            }
        }, TaskScheduler.Default);
    }

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