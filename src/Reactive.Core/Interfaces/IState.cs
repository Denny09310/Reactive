namespace Reactive.Core.Interfaces;

public interface IState : IDisposable
{
    internal abstract void Link(Effect effect);

    internal abstract void Unlink(Effect effect);
}

public interface IState<out TValue> : IState
{
    TValue Get();
}