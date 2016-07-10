namespace StateMechanic
{
    internal class IgnoredTransition<TState> : IInvokableTransition
        where TState : StateBase<TState>, new()
    {
        private readonly ITransitionDelegate<TState> transitionDelegate;
        private readonly TState fromState;
        private readonly IEvent @event;

        bool ITransition.WillAlwaysOccur => true;

        public IgnoredTransition(TState fromState, IEvent @event, ITransitionDelegate<TState> transitionDelegate)
        {
            this.transitionDelegate = transitionDelegate;
            this.fromState = fromState;
            this.@event = @event;
        }

        public bool TryInvoke(EventFireMethod eventFireMethod)
        {
            this.transitionDelegate.IgnoreTransition(this.fromState, this.@event, eventFireMethod);
            return true;
        }

        public override string ToString()
        {
            return $"<IgnoredTransition Event={this.@event}>";
        }
    }
}
