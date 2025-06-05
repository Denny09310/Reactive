namespace Reactive.Core.Interfaces;

public interface IWritableState<TValue> : IState<TValue>
{
    void Set(TValue value);

    void Set(Func<TValue, TValue> updater);
}