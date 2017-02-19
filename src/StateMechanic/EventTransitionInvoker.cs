namespace StateMechanic
{
    internal struct EventTransitionInvoker<TState> : ITransitionInvoker<TState>
        where TState : StateBase<TState>, new()
    {
        private readonly ITransitionDelegate<TState> transitionDelegate;
        private readonly Event @event;
        
        public EventFireMethod EventFireMethod { get; }
		public int EventFireMethodInt => (int)this.EventFireMethod;
        public IEvent Event => this.@event;

        public EventTransitionInvoker(Event @event, EventFireMethod eventFireMethod, ITransitionDelegate<TState> transitionDelegate)
        {
            this.@event = @event;
            this.EventFireMethod = eventFireMethod;
            this.transitionDelegate = transitionDelegate;
        }

        public bool TryInvoke(TState sourceState)
        {
            var customTo = sourceState.HandleEvent(this.Event, null);
            if (customTo != null)
            {
                var invoker = new ForcedTransitionInvoker<TState>(customTo, this.Event, null, this.transitionDelegate);
                // This shouldn't fail
                bool success = invoker.TryInvoke(sourceState);
                Trace.Assert(success, "Forced transition did not succeed");
                return true;
            }

            foreach (var transition in this.@event.GetTransitionsFromState(sourceState))
            {
                if (transition.TryInvoke(this.EventFireMethod))
                    return true;
            }

            return false;
        }
    }

    internal struct EventTransitionInvoker<TState, TEventData> : ITransitionInvoker<TState>
        where TState : StateBase<TState>, new()
    {
        private readonly ITransitionDelegate<TState> transitionDelegate;
        private readonly Event<TEventData> @event;
        private readonly TEventData eventData;
        
        public EventFireMethod EventFireMethod { get; }
		public int EventFireMethodInt => (int)this.EventFireMethod;
        public IEvent Event => this.@event;

        public EventTransitionInvoker(Event<TEventData> @event, EventFireMethod eventFireMethod, TEventData eventData, ITransitionDelegate<TState> transitionDelegate)
        {
            this.@event = @event;
            this.EventFireMethod = eventFireMethod;
            this.transitionDelegate = transitionDelegate;
            this.eventData = eventData;
        }

        public bool TryInvoke(TState sourceState)
        {
            var customTo = sourceState.HandleEvent(this.Event, this.eventData);
            if (customTo != null)
            {
                var invoker = new ForcedTransitionInvoker<TState>(customTo, this.Event, this.eventData, this.transitionDelegate);
                // This shouldn't fail
                bool success = invoker.TryInvoke(sourceState);
                Trace.Assert(success, "Forced transition did not succeed");
                return true;
            }

            foreach (var transition in this.@event.GetTransitionsFromState(sourceState))
            {
                if (transition.TryInvoke(this.eventData, this.EventFireMethod))
                    return true;
            }

            return false;
        }
    }
}

