using System;

namespace StateMechanic
{
    internal class TransitionBuilder<TState, TEventData> : ITransitionBuilder<TState, TEventData>
        where TState : class, IState<TState>
    {
        private readonly TState fromState;
        private readonly Event<TEventData> @event;
        private readonly ITransitionDelegate<TState> transitionDelegate;

        public TransitionBuilder(TState fromState, Event<TEventData> @event, ITransitionDelegate<TState> transitionDelegate)
        {
            this.fromState = fromState;
            this.@event = @event;
            this.transitionDelegate = transitionDelegate;
        }

        public Transition<TState, TEventData> To(TState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            var transition = new Transition<TState, TEventData>(this.fromState, state, this.@event, this.transitionDelegate);
            this.@event.AddTransition(this.fromState, transition, this.fromState.ParentStateMachine);
            this.fromState.AddTransition(transition);
            return transition;
        }

        public DynamicTransition<TState, TEventData> ToDynamic(Func<DynamicSelectorInfo<TState, TEventData>, TState> stateSelector)
        {
            if (stateSelector == null)
                throw new ArgumentNullException(nameof(stateSelector));

            var transition = new DynamicTransition<TState, TEventData>(this.fromState, this.@event, stateSelector, this.transitionDelegate);
            this.@event.AddTransition(this.fromState, transition, this.fromState.ParentStateMachine);
            this.fromState.AddDynamicTransition(transition);
            return transition;
        }
    }
}
