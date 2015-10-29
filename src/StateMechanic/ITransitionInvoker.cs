namespace StateMechanic
{
    internal interface ITransitionInvoker<TState> where TState : IState
    {
        EventFireMethod EventFireMethod { get; }
        IEvent Event { get; }
        bool TryInvoke(TState sourceState);
    }
}
