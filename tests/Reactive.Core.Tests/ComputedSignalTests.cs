namespace Reactive.Core.Tests;

public class ComputedSignalTests
{
    [Fact, Trait("Category", "Computed")]
    public void Computed_Recomputes_On_Dependencies_Change()
    {
        var state = Signal.State(1);
        var computed = Signal.Computed(() => state.Get() * 2);

        Assert.Equal(2, computed.Get());
        state.Set(3);
        Assert.Equal(6, computed.Get());
    }

    [Fact, Trait("Category", "Computed")]
    public void Computed_Value_Is_Not_Reevaluated_When_Dependencies_Unchanged()
    {
        var recomputes = 0;
        var state = Signal.State(10);

        var computed = Signal.Computed(() =>
        {
            recomputes++;
            return state.Get() * 2;
        });

        _ = computed.Get();
        _ = computed.Get();
        _ = computed.Get();

        Assert.Equal(1, recomputes);
    }

    [Fact, Trait("Category", "Computed")]
    public void Computed_Chained_Dependency_Reacts_To_Inner_Change()
    {
        var baseSignal = Signal.State(2);
        var inner = Signal.Computed(() => baseSignal.Get() + 1);
        var outer = Signal.Computed(() => inner.Get() * 2);

        Assert.Equal(6, outer.Get());

        baseSignal.Set(3);

        Assert.Equal(8, outer.Get());
    }
}
