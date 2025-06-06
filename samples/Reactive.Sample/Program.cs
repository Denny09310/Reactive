using Reactive.Core;
using static Reactive.Core.Extensions.Reactivity;

// 1. Define a signal (list of strings) and a Computed<int> for the count:
var users = Signal.State(new List<string> { "Jasmine", "John" });
var count = Signal.Computed(() => users.Get().Count);

// 2. Create an effect that prints both users and count:
Effect(() =>
{
    Console.WriteLine($"[{string.Join(", ", users.Get())}], Count: {count.Get()}");
});

// 3. Update the list inside :
users.Update(prev => [.. prev, "Smith"]);

// Expected console output (exactly two lines):
// [Jasmine, Jonh], Count: 2
// [Jasmine, Jonh, Smith], Count: 3

