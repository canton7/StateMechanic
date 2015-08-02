namespace StateMechanic
{
    internal interface IStateDelegate<TState>
    {
        TState InitialState { get; }
        TState CurrentState { get; }
        void ForceTransition(TState pretendFromState, TState pretendToState, TState toState, IEvent @event);
    }
}
