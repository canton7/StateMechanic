using System;

namespace StateMechanic
{
    internal class TransitionBuilder<TState> : ITransitionBuilder<TState>
        where TState : StateBase<TState>, new()
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
            this.@event.AddTransition(this.fromState, transition, this.fromState.ParentStateMachine.TopmostStateMachineInternal);
            this.fromState.AddTransition(transition);
            return transition;
        }

        public DynamicTransition<TState> ToDynamic(Func<DynamicSelectorInfo<TState>, TState> stateSelector)
        {
            if (stateSelector == null)
                throw new ArgumentNullException(nameof(stateSelector));

            var transition = new DynamicTransition<TState>(this.fromState, this.@event, stateSelector, this.transitionDelegate);
            this.@event.AddTransition(this.fromState, transition, this.fromState.ParentStateMachine.TopmostStateMachineInternal);
            this.fromState.AddTransition(transition);
            return transition;
        }
    }

    internal class TransitionBuilder<TState, TEventData> : ITransitionBuilder<TState, TEventData>
        where TState : StateBase<TState>, new()
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
            this.@event.AddTransition(this.fromState, transition, this.fromState.ParentStateMachine.TopmostStateMachineInternal);
            this.fromState.AddTransition(transition);
            return transition;
        }

        public DynamicTransition<TState, TEventData> ToDynamic(Func<DynamicSelectorInfo<TState, TEventData>, TState> stateSelector)
        {
            if (stateSelector == null)
                throw new ArgumentNullException(nameof(stateSelector));

            var transition = new DynamicTransition<TState, TEventData>(this.fromState, this.@event, stateSelector, this.transitionDelegate);
            this.@event.AddTransition(this.fromState, transition, this.fromState.ParentStateMachine.TopmostStateMachineInternal);
            this.fromState.AddTransition(transition);
            return transition;
        }
    }
}

