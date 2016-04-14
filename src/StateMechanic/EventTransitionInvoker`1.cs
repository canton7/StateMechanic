namespace StateMechanic
{
    internal class EventTransitionInvoker<TState> : ITransitionInvoker<TState> where TState : IState
    {
        private readonly Event @event;

        public EventFireMethod EventFireMethod { get; }
        public IEvent Event => this.@event;

        public EventTransitionInvoker(Event @event, EventFireMethod eventFireMethod)
        {
            this.@event = @event;
            this.EventFireMethod = eventFireMethod;
        }

        public bool TryInvoke(TState sourceState)
        {
            foreach (var transition in this.@event.GetTransitionsFromState(sourceState))
            {
                if (transition.TryInvoke())
                    return true;
            }

            return false;
        }
    }
}
