using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    internal class StateInner<TState>
    {
        private readonly Dictionary<IEvent, ITransition<TState>> transitions = new Dictionary<IEvent, ITransition<TState>>();
        public string Name { get; private set; }

        internal StateInner(string name)
        {
            this.Name = name;
        }

        public ITransitionBuilder<TState> AddTransitionOn(TState fromState, Event evt)
        {
            return new TransitionBuilder<TState>(fromState, evt, transition => this.AddTransition(evt, transition));
        }

        public ITransitionBuilder<TState, TEventData> AddTransitionOn<TEventData>(TState fromState, Event<TEventData> evt)
        {
            return new TransitionBuilder<TState, TEventData>(fromState, evt, transition => this.AddTransition(evt, transition));
        }

        internal void AddTransition(IEvent evt, ITransition<TState> transition)
        {
            this.transitions.Add(evt, transition);
        }

        internal bool TryFireEvent(IEvent evt)
        {
            ITransition<TState> transition;
            if (!this.transitions.TryGetValue(evt, out transition))
                return false;

            // Something something something

            return true;
        }
    }

    public class State : IState
    {
        private readonly StateInner<State> innerState;

        internal State(string name)
        {
            this.innerState = new StateInner<State>(name);
        }

        public ITransitionBuilder<State> AddTransitionOn(Event evt)
        {
            return this.innerState.AddTransitionOn(this, evt);
        }

        public ITransitionBuilder<State, TEventData> AddTransitionOn<TEventData>(Event<TEventData> evt)
        {
            return this.innerState.AddTransitionOn<TEventData>(this, evt);
        }

        internal bool TryFireEvent(IEvent evt)
        {
            return this.innerState.TryFireEvent(evt);
        }
    }

    public class State<TStateData> : IState
    {
        private readonly StateInner<State<TStateData>> innerState;

        internal State(string name)
        {
            this.innerState = new StateInner<State<TStateData>>(name);
        }

        public ITransitionBuilder<State<TStateData>> AddTransitionOn(Event evt)
        {
            return this.innerState.AddTransitionOn(this, evt);
        }

        public ITransitionBuilder<State<TStateData>, TEventData> AddTransitionOn<TEventData>(Event<TEventData> evt)
        {
            return this.innerState.AddTransitionOn<TEventData>(this, evt);
        }

        internal bool TryFireEvent(IEvent evt)
        {
            return this.innerState.TryFireEvent(evt);
        }
    }
}
