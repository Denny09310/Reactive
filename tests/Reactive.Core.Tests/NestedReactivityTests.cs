using static Reactive.Core.Extensions.Reactivity;

namespace Reactive.Core.Tests;

public class NestedReactivityTests
{
    [Fact, Trait("Category", "Nested")]
    public void Nested_Computeds_And_Effects_Behave_Correctly()
    {
        var output = new List<string>();

        var first = Signal.State("Dennis");
        var last = Signal.State("Ritchie");

        var full = Signal.Computed(() => $"{first.Get()} {last.Get()}");
        var greeting = Signal.Computed(() => $"Hello, {full.Get()}!");

        Effect(() =>
        {
            output.Add(greeting.Get());
        });

        first.Set("Linus");
        last.Set("Torvalds");

        Assert.Equal(["Hello, Dennis Ritchie!", "Hello, Linus Ritchie!", "Hello, Linus Torvalds!"], output);
    }


    [Fact, Trait("Category", "Nested")]
    public void Deeply_Nested_Signals_Recompute_Correctly()
    {
        var a = Signal.State(1);
        var b = Signal.Computed(() => a.Get() + 1);
        var c = Signal.Computed(() => b.Get() + 1);
        var d = Signal.Computed(() => c.Get() + 1);

        Assert.Equal(4, d.Get());

        a.Set(2);

        Assert.Equal(5, d.Get());
    }
}
