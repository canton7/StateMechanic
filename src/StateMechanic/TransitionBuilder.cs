using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    internal class TransitionBuilder<TState> : ITransitionBuilder<TState> where TState : IState<TState>
    {
        private readonly TState fromState;
        private readonly Event evt;
        private readonly ITransitionDelegate<TState> transitionRepository;

        public TransitionBuilder(TState fromState, Event evt, ITransitionDelegate<TState> transitionRepository)
        {
            this.fromState = fromState;
            this.evt = evt;
            this.transitionRepository = transitionRepository;
        }

        ITransition<TState> ITransitionBuilder<TState>.To(TState state)
        {
            var transition = new Transition<TState>(this.fromState, state, this.evt, this.transitionRepository);
            this.transitionRepository.AddTransition(this.evt, transition);
            return transition;
        }
    }

    internal class TransitionBuilder<TState, TEventData> : ITransitionBuilder<TState, TEventData> where TState : IState<TState>
    {
        private readonly TState fromState;
        private readonly Event<TEventData> evt;
        private readonly ITransitionDelegate<TState> transitionRepository;

        public TransitionBuilder(TState fromState, Event<TEventData> evt, ITransitionDelegate<TState> transitionRepository)
        {
            this.fromState = fromState;
            this.evt = evt;
            this.transitionRepository = transitionRepository;
        }

        ITransition<TState, TEventData> ITransitionBuilder<TState, TEventData>.To(TState state)
        {
            var transition = new Transition<TState, TEventData>(this.fromState, state, this.evt, this.transitionRepository);
            this.transitionRepository.AddTransition(this.evt, transition);
            return transition;
        }
    }
}
