using static Reactive.Core.Extensions.Reactivity;
using static Reactive.Core.Utils.Tracker;

namespace Reactive.Core.Tests;

public class UntrackedAccessTests
{
    [Fact, Trait("Category", "Untracked")]
    public void Effect_With_Untracked_Should_Not_Run()
    {
        var runCount = 0;
        var a = Signal.State(1);

        Effect(() =>
        {
            runCount++;
            var _ = Untracked(() => a.Get());
        });

        a.Set(2);

        Assert.Equal(1, runCount);
    }

    [Fact, Trait("Category", "Untracked")]
    public void Mixing_Tracked_And_Untracked_Still_Reacts()
    {
        var count = 0;
        var tracked = Signal.State(1);
        var untracked = Signal.State(2);

        Effect(() =>
        {
            _ = tracked.Get();
            _ = Untracked(() => untracked.Get());
            count++;
        });

        tracked.Set(5);
        untracked.Set(8);

        Assert.Equal(2, count);
    }
}
