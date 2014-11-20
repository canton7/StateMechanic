using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public delegate void StateHandler(StateHandlerInfo<State> info);
    public delegate void StateHandler<TStateData>(StateHandlerInfo<State<TStateData>> info);

    internal class StateInner<TState> where TState : IState<TState>
    {
        private readonly ITransitionDelegate<TState> transitionRepository;

        public string Name { get; private set; }

        internal StateInner(string name, ITransitionDelegate<TState> transitionRepository)
        {
            this.Name = name;
            this.transitionRepository = transitionRepository;
        }

        public ITransitionBuilder<TState> AddTransitionOn(TState fromState, Event evt)
        {
            return new TransitionBuilder<TState>(fromState, evt, this.transitionRepository);
        }

        public ITransitionBuilder<TState, TEventData> AddTransitionOn<TEventData>(TState fromState, Event<TEventData> evt)
        {
            return new TransitionBuilder<TState, TEventData>(fromState, evt, this.transitionRepository);
        }
    }

    public class State : IState<State>
    {
        private readonly StateInner<State> innerState;

        public StateHandler OnEntry { get; set; }
        public StateHandler OnExit { get; set; }

        public string Name { get { return this.innerState.Name; } }

        internal State(string name, ITransitionDelegate<State> transitionRepository)
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

        public State WithEntry(StateHandler onEntry)
        {
            this.OnEntry = onEntry;
            return this;
        }

        public State WithExit(StateHandler onExit)
        {
            this.OnExit = onExit;
            return this;
        }

        void IState<State>.FireOnEntry(StateHandlerInfo<State> info)
        {
            var onEntry = this.OnEntry;
            if (onEntry != null)
                onEntry(info);
        }

        void IState<State>.FireOnExit(StateHandlerInfo<State> info)
        {
            var onExit = this.OnExit;
            if (onExit != null)
                onExit(info);
        }
    }

    public class State<TStateData> : IState<State<TStateData>>
    {
        private readonly StateInner<State<TStateData>> innerState;

        public TStateData Data { get; set; }
        public StateHandler<TStateData> OnEntry { get; set; }
        public StateHandler<TStateData> OnExit { get; set; }

        public string Name { get { return this.innerState.Name; } }

        internal State(string name, ITransitionDelegate<State<TStateData>> transitionRepository)
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

        public State<TStateData> WithData(TStateData data)
        {
            this.Data = data;
            return this;
        }

        public State<TStateData> WithEntry(StateHandler<TStateData> onEntry)
        {
            this.OnEntry = onEntry;
            return this;
        }

        public State<TStateData> WithExit(StateHandler<TStateData> onExit)
        {
            this.OnExit = onExit;
            return this;
        }

        void IState<State<TStateData>>.FireOnEntry(StateHandlerInfo<State<TStateData>> info)
        {
            var onEntry = this.OnEntry;
            if (onEntry != null)
                onEntry(info);
        }

        void IState<State<TStateData>>.FireOnExit(StateHandlerInfo<State<TStateData>> info)
        {
            var onExit = this.OnExit;
            if (onExit != null)
                onExit(info);
        }
    }
}
