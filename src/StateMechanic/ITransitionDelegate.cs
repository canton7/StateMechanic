namespace StateMechanic
{
    internal interface ITransitionDelegate<TState> where TState : IState
    {
        void UpdateCurrentState(TState from, TState state, IEvent @event, bool isInnerTransition);
    }
}
