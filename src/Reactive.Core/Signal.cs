namespace Reactive.Core;

public static class Signal
{
    public static Computed<T> Computed<T>(Func<T> compute) => new(compute);

    public static Resource<T> Resource<T>(Func<CancellationToken, Task<T>> loader, bool start = true) => new(loader, start);

    public static Resource<TArgs, TValue> Resource<TArgs, TValue>(Func<TArgs> args, Func<TArgs, CancellationToken, Task<TValue>> loader, bool start = true) => new(args, loader, start);

    public static State<T> State<T>(T initialValue) => new(initialValue);
}