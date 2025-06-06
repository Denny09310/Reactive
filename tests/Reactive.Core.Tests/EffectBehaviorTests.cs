using static Reactive.Core.Extensions.Reactivity;

namespace Reactive.Core.Tests;

public class EffectBehaviorTests
{
    [Fact, Trait("Category", "Effect")]
    public void Effect_Runs_Initially()
    {
        var output = new List<string>();
        var signal = Signal.State("A");

        Effect(() => output.Add(signal.Get()));

        Assert.Equal(["A"], output);
    }

    [Fact, Trait("Category", "Effect")]
    public void Effect_Runs_On_State_Change()
    {
        var output = new List<string>();
        var signal = Signal.State("A");

        Effect(() => output.Add(signal.Get()));

        signal.Set("B");

        Assert.Equal(["A", "B"], output);
    }

    [Fact, Trait("Category", "Effect")]
    public void Effect_Does_Not_ReRun_If_Dependency_Stays_The_Same()
    {
        var count = 0;
        var state = Signal.State("init");

        Effect(() =>
        {
            _ = state.Get();
            count++;
        });

        state.Set("init");

        Assert.Equal(1, count);
    }

    [Fact, Trait("Category", "Effect")]
    public void Effect_Runs_Only_Once_Per_Batch()
    {
        var runCount = 0;
        var state = Signal.State(1);
        var computed = Signal.Computed(() => state.Get() + 1);

        Effect(() =>
        {
            var _ = state.Get();
            var __ = computed.Get();
            runCount++;
        });

        state.Set(2);

        Assert.Equal(2, runCount);
    }

    [Fact, Trait("Category", "Effect")]
    public void Multiple_Effects_Share_State_And_Run_Independently()
    {
        var aLog = new List<int>();
        var bLog = new List<int>();

        var state = Signal.State(5);

        Effect(() => aLog.Add(state.Get()));
        Effect(() => bLog.Add(state.Get() + 1));

        state.Set(6);

        Assert.Equal([5, 6], aLog);
        Assert.Equal([6, 7], bLog);
    }
}
