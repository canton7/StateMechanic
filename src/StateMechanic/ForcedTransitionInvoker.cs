namespace StateMechanic
{
    internal class ForcedTransitionInvoker<TState> : ITransitionInvoker<TState> where TState : IState
    {
        private readonly TState toState;
        private readonly ITransitionDelegate<TState> transitionDelegate;

        public EventFireMethod EventFireMethod { get; }
        public IEvent Event { get; }

        public ForcedTransitionInvoker(TState toState, IEvent @event, ITransitionDelegate<TState> transitionDelegate)
        {
            this.toState = toState;
            this.EventFireMethod = EventFireMethod.Fire;
            this.Event = @event;
            this.transitionDelegate = transitionDelegate;
        }

        public bool TryInvoke(TState sourceState)
        {
            // No transition data (no handler)
            this.transitionDelegate.CoordinateTransition<object>(sourceState, this.toState, this.Event, false, null, null);
            return true;
        }
    }
}
