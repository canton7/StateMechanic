namespace StateMechanic
{
    internal interface IStateMachine<TState> : IStateMachine, IEventDelegate where TState : class, IState<TState>
    {
        new TState CurrentChildState { get; }
        new TState InitialState { get; }
        new TState CurrentState { get; }
        bool RequestEventFire(ITransitionInvoker<TState> transitionInvoker, bool overrideNoThrow = false);
        void SetCurrentState(TState state);
    }
}
