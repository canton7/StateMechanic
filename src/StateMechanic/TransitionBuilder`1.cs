using System;

namespace StateMechanic
{
    internal class TransitionBuilder<TState> : ITransitionBuilder<TState> where TState : class, IState<TState>
    {
        private readonly TState fromState;
        private readonly Event @event;
        private readonly ITransitionDelegate<TState> transitionDelegate;

        public TransitionBuilder(TState fromState, Event @event, ITransitionDelegate<TState> transitionDelegate)
        {
            this.fromState = fromState;
            this.@event = @event;
            this.transitionDelegate = transitionDelegate;
        }

        public Transition<TState> To(TState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            var transition = new Transition<TState>(this.fromState, state, this.@event, this.transitionDelegate);
            this.@event.AddTransition(this.fromState, transition, this.fromState.ParentStateMachine);
            this.fromState.AddTransition(transition);
            return transition;
        }

        public DynamicTransition<TState> ToDynamic(Func<DynamicSelectorInfo<TState>, TState> stateSelector)
        {
            if (stateSelector == null)
                throw new ArgumentNullException(nameof(stateSelector));

            var transition = new DynamicTransition<TState>(this.fromState, this.@event, stateSelector, this.transitionDelegate);
            this.@event.AddTransition(this.fromState, transition, this.fromState.ParentStateMachine);
            this.fromState.AddTransition(transition);
            return transition;
        }
    }
}
