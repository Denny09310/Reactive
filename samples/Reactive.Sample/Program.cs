using Reactive.Core;
using static Reactive.Core.Extensions.Reactivity;

var users = Signal.State<List<string>>(["Jasmine", "Jonh"]);

Effect(() =>
{
    Console.WriteLine($"[{string.Join(", ", users.Get())}]");
});

users.Set(u => [..u, "Smith"]);