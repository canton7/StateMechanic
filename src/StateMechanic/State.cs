using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    internal interface ITransitionRepository<TState>
    {
        void AddTransition(Event evt, Transition<TState> transition);

        void AddTransition<TEventData>(Event<TEventData> evt, Transition<TState, TEventData> transition);
    }

    internal class StateInner<TState>
    {
        public string Name { get; private set; }
        private readonly ITransitionRepository<TState> transitionRepository;


        internal StateInner(string name, ITransitionRepository<TState> transitionRepository)
        {
            this.Name = name;
            this.transitionRepository = transitionRepository;
        }

        public ITransitionBuilder<TState> AddTransitionOn(TState fromState, Event evt)
        {
            return new TransitionBuilder<TState>(fromState, evt, this.transitionRepository.AddTransition);
        }

        public ITransitionBuilder<TState, TEventData> AddTransitionOn<TEventData>(TState fromState, Event<TEventData> evt)
        {
            return new TransitionBuilder<TState, TEventData>(fromState, evt, this.transitionRepository.AddTransition<TEventData>);
        }
    }

    public class State : IState
    {
        private readonly StateInner<State> innerState;

        public string Name { get { return this.innerState.Name; } }

        internal State(string name, ITransitionRepository<State> transitionRepository)
        {
            this.innerState = new StateInner<State>(name, transitionRepository);
        }

        public ITransitionBuilder<State> AddTransitionOn(Event evt)
        {
             return this.innerState.AddTransitionOn(this, evt);
        }

        public ITransitionBuilder<State, TEventData> AddTransitionOn<TEventData>(Event<TEventData> evt)
        {
            return this.innerState.AddTransitionOn<TEventData>(this, evt);
        }
    }

    public class State<TStateData> : IState
    {
        private readonly StateInner<State<TStateData>> innerState;

        public string Name { get { return this.innerState.Name; } }

        internal State(string name, ITransitionRepository<State<TStateData>> transitionRepository)
        {
            this.innerState = new StateInner<State<TStateData>>(name, transitionRepository);
        }

        public ITransitionBuilder<State<TStateData>> AddTransitionOn(Event evt)
        {
            return this.innerState.AddTransitionOn(this, evt);
        }

        public ITransitionBuilder<State<TStateData>, TEventData> AddTransitionOn<TEventData>(Event<TEventData> evt)
        {
            return this.innerState.AddTransitionOn<TEventData>(this, evt);
        }
    }
}
