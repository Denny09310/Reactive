namespace Reactive.Core.Utils;

public static class Tracker
{
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