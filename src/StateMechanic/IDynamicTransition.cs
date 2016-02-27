namespace StateMechanic
{
    public interface IDynamicTransition<out TState> where TState : IState
    {
        TState From { get; }
        IEvent Event { get; }
    }
}
