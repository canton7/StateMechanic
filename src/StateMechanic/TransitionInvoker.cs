namespace StateMechanic
{
    internal interface ITransitionInvoker<TState> where TState : IState
    {
        EventFireMethod EventFireMethod { get; }
        IEvent Event { get; }
        bool TryInvoke(TState sourceState);
    }

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
            var transitions = this.@event.GetTransitionsForState(sourceState);
            if (transitions == null)
                return false;

            foreach (var transition in transitions)
            {
                if (transition.TryInvoke())
                    return true;
            }

            return false;
        }
    }

    internal class EventTransitionInvokerWithData<TState, TEventData> : ITransitionInvoker<TState> where TState : IState
    {
        private readonly Event<TEventData> @event;
        private readonly TEventData eventData;

        public EventFireMethod EventFireMethod { get; }
        public IEvent Event => this.@event;

        public EventTransitionInvokerWithData(Event<TEventData> @event, EventFireMethod eventFireMethod, TEventData eventData)
        {
            this.@event = @event;
            this.EventFireMethod = eventFireMethod;
            this.eventData = eventData;
        }

        public bool TryInvoke(TState sourceState)
        {
            var transitions = this.@event.GetTransitionsForState(sourceState);
            if (transitions == null)
                return false;

            foreach (var transition in transitions)
            {
                if (transition.TryInvoke(this.eventData))
                    return true;
            }

            return false;
        }
    }

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
