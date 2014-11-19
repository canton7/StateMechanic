using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public interface ITransitionBuilder<TState> where TState : IState<TState>
    {
        Transition<TState> To(TState state);
    }

    internal class TransitionBuilder<TState> : ITransitionBuilder<TState> where TState : IState<TState>
    {
        private readonly TState fromState;
        private readonly Event evt;
        private readonly ITransitionRepository<TState> transitionRepository;

        public TransitionBuilder(TState fromState, Event evt, ITransitionRepository<TState> transitionRepository)
        {
            this.fromState = fromState;
            this.evt = evt;
            this.transitionRepository = transitionRepository;
        }

        Transition<TState> ITransitionBuilder<TState>.To(TState state)
        {
            var transition = new Transition<TState>(this.fromState, state, this.evt, this.transitionRepository);
            this.transitionRepository.AddTransition(this.evt, transition);
            return transition;
        }
    }

    public interface ITransitionBuilder<TState, TEventData> where TState : IState<TState>
    {
        Transition<TState, TEventData> To(TState state);
    }

    internal class TransitionBuilder<TState, TEventData> : ITransitionBuilder<TState, TEventData> where TState : IState<TState>
    {
        private readonly TState fromState;
        private readonly Event<TEventData> evt;
        private readonly ITransitionRepository<TState> transitionRepository;

        public TransitionBuilder(TState fromState, Event<TEventData> evt, ITransitionRepository<TState> transitionRepository)
        {
            this.fromState = fromState;
            this.evt = evt;
            this.transitionRepository = transitionRepository;
        }

        Transition<TState, TEventData> ITransitionBuilder<TState, TEventData>.To(TState state)
        {
            var transition = new Transition<TState, TEventData>(this.fromState, state, this.evt, this.transitionRepository);
            this.transitionRepository.AddTransition(this.evt, transition);
            return transition;
        }
    }
}
