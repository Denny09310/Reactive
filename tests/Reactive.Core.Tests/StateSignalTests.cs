using static Reactive.Core.Extensions.Reactivity;

namespace Reactive.Core.Tests;

public class StateSignalTests
{
    [Fact, Trait("Category", "State")]
    public void State_Setting_Same_Value_Does_Not_Trigger_Change()
    {
        var runCount = 0;
        var signal = Signal.State(42);

        Effect(() => { _ = signal.Get(); runCount++; });

        signal.Set(42);

        Assert.Equal(1, runCount);
    }

    [Fact, Trait("Category", "State")]
    public async Task State_Update_Triggers_Effect()
    {
        var values = new List<int>();
        var state = Signal.State(0);

        Effect(() => values.Add(state.Get()));

        state.Update(prev => prev + 1);
        state.Update(prev => prev + 1);

        Assert.Equal([0, 1, 2], values);
    }
}