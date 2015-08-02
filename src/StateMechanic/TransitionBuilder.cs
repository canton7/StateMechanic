using System;

namespace StateMechanic
{
    internal class TransitionBuilder<TState> : ITransitionBuilder<TState> where TState : class, IState<TState>
    {
        private readonly TState fromState;
        private readonly Event evt;
        private readonly ITransitionDelegate<TState> transitionDelegate;

        public TransitionBuilder(TState fromState, Event @event, ITransitionDelegate<TState> transitionDelegate)
        {
            this.fromState = fromState;
            this.evt = @event;
            this.transitionDelegate = transitionDelegate;
        }

        public Transition<TState> To(TState state)
        {
            if (state == null)
                throw new ArgumentNullException("state");
            var transition = Transition.Create<TState>(this.fromState, state, this.evt, this.transitionDelegate);
            this.evt.AddTransition(this.fromState, transition);
            this.fromState.AddTransition(transition);
            return transition;
        }
    }

    internal class TransitionBuilder<TState, TEventData> : ITransitionBuilder<TState, TEventData> where TState : class, IState<TState>
    {
        private readonly TState fromState;
        private readonly Event<TEventData> evt;
        private readonly ITransitionDelegate<TState> transitionDelegate;

        public TransitionBuilder(TState fromState, Event<TEventData> @event, ITransitionDelegate<TState> transitionDelegate)
        {
            this.fromState = fromState;
            this.evt = @event;
            this.transitionDelegate = transitionDelegate;
        }

        public Transition<TState, TEventData> To(TState state)
        {
            if (state == null)
                throw new ArgumentNullException("state");
            var transition = Transition.Create<TState, TEventData>(this.fromState, state, this.evt, this.transitionDelegate);
            this.evt.AddTransition(this.fromState, transition);
            this.fromState.AddTransition(transition);
            return transition;
        }
    }
}
