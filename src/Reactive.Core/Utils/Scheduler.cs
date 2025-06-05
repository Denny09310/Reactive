namespace Reactive.Core.Utils;

public static class Scheduler
{
    private static readonly HashSet<Action> _effects = [];
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