namespace StateMechanic
{
    internal class EventTransitionInvoker<TState> : ITransitionInvoker<TState> where TState : StateBase<TState>, new()
    {
        private readonly Event @event;
        private readonly ITransitionDelegate<TState> transitionDelegate;

        public EventFireMethod EventFireMethod { get; }
        public IEvent Event => this.@event;

        public EventTransitionInvoker(Event @event, EventFireMethod eventFireMethod, ITransitionDelegate<TState> transitionDelegate)
        {
            this.@event = @event;
            this.EventFireMethod = eventFireMethod;
            this.transitionDelegate = transitionDelegate;
        }

        public bool TryInvoke(TState sourceState)
        {
            var customTo = sourceState.HandleEvent(this.Event);
            if (customTo != null)
            {
                var invoker = new ForcedTransitionInvoker<TState>(customTo, this.Event, this.transitionDelegate);
                // This shouldn't fail
                bool success = invoker.TryInvoke(sourceState);
                Trace.Assert(success, "Forced transition did not succeed");
                return true;
            }

            foreach (var transition in this.@event.GetTransitionsFromState(sourceState))
            {
                if (transition.TryInvoke())
                    return true;
            }

            return false;
        }
    }
}
