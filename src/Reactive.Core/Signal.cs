namespace Reactive.Core;

public static class Signal
{
    public static Computed<T> Computed<T>(Func<T> compute) => new(compute);

    public static State<T> State<T>(T initialValue) => new(initialValue);
}