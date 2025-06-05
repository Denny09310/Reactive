using Reactive.Core.Interfaces;

namespace Reactive.Core;

public class Effect
{
    [ThreadStatic]
    internal static Effect? Current;

    private readonly Action _callback;
    private readonly HashSet<IState> _dependencies = [];

    public Effect(Action callback)
    {
        _callback = callback;
        Run();
    }

    public void Invalidate()
    {
        Run();
    }

    public void Register(IState signal)
    {
        _dependencies.Add(signal);
        signal.Link(this);
    }

    public void Run()
    {
        foreach (var dependency in _dependencies)
        {
            dependency.Unlink(this);
        }

        _dependencies.Clear();

        var prev = Current;
        Current = this;

        try
        {
            _callback();
        }
        finally
        {
            Current = prev;
        }
    }
}