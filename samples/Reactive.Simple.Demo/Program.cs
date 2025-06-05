using Reactive.Core;
using static Reactive.Core.Extensions.Reactivity;

var users = Signal.State<List<string>>(["Dennis", "Ilio"]);

Effect(() =>
{
    Console.WriteLine($"[{string.Join(", ", users.Get())}]");
});

users.Set(u => [..u, "Enea"]);