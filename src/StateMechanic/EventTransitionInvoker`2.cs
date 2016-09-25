namespace StateMechanic
{
    internal class EventTransitionInvoker<TState, TEventData> : ITransitionInvoker<TState> where TState : IState
    {
        private readonly Event<TEventData> @event;
        private readonly TEventData eventData;

        public EventFireMethod EventFireMethod { get; }
        public IEvent Event => this.@event;

        public EventTransitionInvoker(Event<TEventData> @event, EventFireMethod eventFireMethod, TEventData eventData)
        {
            this.@event = @event;
            this.EventFireMethod = eventFireMethod;
            this.eventData = eventData;
        }

        public bool TryInvoke(TState sourceState)
        {
            foreach (var transition in this.@event.GetTransitionsFromState(sourceState))
            {
                if (transition.TryInvoke(this.eventData, this.EventFireMethod))
                    return true;
            }

            return false;
        }
    }
}
