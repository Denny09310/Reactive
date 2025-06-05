namespace Reactive.Core.Extensions;

public static class Reactivity
{
    public static Effect Effect(Action callback) => new(callback);

    public static Resource<T> Resource<T>(Func<CancellationToken, Task<T>> loader, bool start = true) => new(loader, start);

    public static Resource<TArgs, TValue> Resource<TArgs, TValue>(Func<TArgs> args, Func<TArgs, CancellationToken, Task<TValue>> loader, bool start = true) => new(args, loader, start);
}