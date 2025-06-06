using static Reactive.Core.Extensions.Reactivity;
using static Reactive.Core.Utils.Tracker;

namespace Reactive.Core.Tests;

public class ReactiveTests
{
    [Fact]
    public void Changing_Only_Deep_Dependency_Recomputes_Effect_Once()
    {
        var runs = 0;

        var a = Signal.State(1);
        var b = Signal.Computed(() => a.Get() + 1);
        var c = Signal.Computed(() => b.Get() * 2);

        Effect(() =>
        {
            _ = c.Get();
            runs++;
        });

        a.Set(2);

        Assert.Equal(2, runs); // Initial + after batch, not more
    }

    [Fact]
    public void Computed_Recomputes_On_Dependencies_Change()
    {
        var state = Signal.State(1);
        var computed = Signal.Computed(() => state.Get() * 2);

        Assert.Equal(2, computed.Get());

        state.Set(3);

        Assert.Equal(6, computed.Get());
    }

    [Fact]
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

        Assert.Equal(1, recomputes); // Cached value reused
    }

    [Fact]
    public void Effect_Runs_Initially()
    {
        var output = new List<string>();
        var signal = Signal.State("A");

        Effect(() =>
        {
            output.Add(signal.Get());
        });

        Assert.Equal(["A"], output);
    }

    [Fact]
    public void Effect_Runs_On_State_Change()
    {
        var output = new List<string>();
        var signal = Signal.State("A");

        Effect(() =>
        {
            output.Add(signal.Get());
        });

        signal.Set("B");

        Assert.Equal(["A", "B"], output);
    }

    [Fact]
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

        Assert.Equal(2, runCount); // Initial + batch
    }

    [Fact]
    public void Effect_Triggered_By_Computed_Change()
    {
        var values = new List<int>();
        var state = Signal.State(1);
        var computed = Signal.Computed(() => state.Get() * 10);

        Effect(() =>
        {
            values.Add(computed.Get());
        });

        state.Set(2);
        state.Set(3);

        Assert.Equal([10, 20, 30], values);
    }

    [Fact]
    public void Effect_With_Indirect_And_Direct_Signal_Dependency_Runs_Once()
    {
        var logs = new List<string>();
        var names = Signal.State(new List<string> { "Ada", "Grace" });
        var count = Signal.Computed(() => names.Get().Count);

        Effect(() =>
        {
            logs.Add($"Names: {string.Join(", ", names.Get())} | Count: {count.Get()}");
        });

        names.Update(prev => [.. prev, "Margaret"]);

        Assert.Equal(2, logs.Count); // Initial + one update
        Assert.Equal("Names: Ada, Grace | Count: 2", logs[0]);
        Assert.Equal("Names: Ada, Grace, Margaret | Count: 3", logs[1]);
    }

    [Fact]
    public void Effect_With_Untracked_Should_Not_Run()
    {
        var runCount = 0;
        var a = Signal.State(1);

        Effect(() =>
        {
            runCount++;
            var _ = Untracked(() => a.Get());
            Console.WriteLine("This should not run more than once.");
        });

        a.Set(2);

        Assert.Equal(1, runCount);
    }

    [Fact]
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

    [Fact]
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

    [Fact]
    public void No_Duplicate_Execution_If_Value_Does_Not_Change()
    {
        var runs = 0;
        var state = Signal.State("foo");

        Effect(() =>
        {
            _ = state.Get();
            runs++;
        });

        state.Set("foo"); // No real change

        Assert.Equal(1, runs); // Should not re-run
    }
}