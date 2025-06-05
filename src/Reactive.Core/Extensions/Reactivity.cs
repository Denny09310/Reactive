namespace Reactive.Core.Extensions;

public static class Reactivity
{
    public static Effect Effect(Action callback) => new(callback);
}