using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public delegate void StateHandler(StateHandlerInfo<State> info);
    public delegate void StateHandler<TStateData>(StateHandlerInfo<State<TStateData>> info);

    internal class StateInner<TState> where TState : class, IState<TState>
    {
        private readonly ITransitionDelegate<TState> transitionDelegate;
        private readonly List<ITransition<TState>> transitions = new List<ITransition<TState>>();

        public string Name { get; private set; }
        public IReadOnlyList<ITransition<TState>> Transitions { get { return this.transitions.AsReadOnly(); } }

        internal StateInner(string name, ITransitionDelegate<TState> transitionDelegate)
        {
            this.Name = name;
            this.transitionDelegate = transitionDelegate;
        }

        public ITransitionBuilder<TState> AddTransitionOn(TState fromState, Event evt)
        {
            return new TransitionBuilder<TState>(fromState, evt, this.transitionDelegate);
        }

        public ITransitionBuilder<TState, TEventData> AddTransitionOn<TEventData>(TState fromState, Event<TEventData> evt)
        {
            return new TransitionBuilder<TState, TEventData>(fromState, evt, this.transitionDelegate);
        }

        public Transition<TState> AddInnerSelfTransitionOn(TState fromAndToState, Event evt)
        {
            var transition = Transition.CreateInner<TState>(fromAndToState, evt, this.transitionDelegate);
            evt.AddTransition(fromAndToState, transition);
            this.AddTransition(transition);
            return transition;
        }

        public Transition<TState, TEventData> AddInnerSelfTransitionOn<TEventData>(TState fromAndToState, Event<TEventData> evt)
        {
            var transition = Transition.CreateInner<TState, TEventData>(fromAndToState, evt, this.transitionDelegate);
            evt.AddTransition(fromAndToState, transition);
            this.AddTransition(transition);
            return transition;
        }

        public void AddTransition(ITransition<TState> transition)
        {
            this.transitions.Add(transition);
        }
    }

    public class State : IState<State>
    {
        private readonly StateInner<State> innerState;

        public StateHandler OnEntry { get; set; }
        public StateHandler OnExit { get; set; }

        public string Name { get { return this.innerState.Name; } }
        public StateMachine ChildStateMachine { get; private set; }
        public StateMachine ParentStateMachine { get; private set; }
        public IReadOnlyList<ITransition<State>> Transitions { get { return this.innerState.Transitions; } }
        IStateMachine IState.ChildStateMachine { get { return this.ChildStateMachine; } }
        IStateMachine IState.ParentStateMachine { get { return this.ParentStateMachine; } }
        IReadOnlyList<ITransition<IState>> IState.Transitions { get { return this.innerState.Transitions; } }
        IStateMachine<State> IState<State>.ParentStateMachine { get { return this.ParentStateMachine; } }

        internal State(string name, StateMachine parentStateMachine)
        {
            this.ParentStateMachine = parentStateMachine;
            this.innerState = new StateInner<State>(name, parentStateMachine);
        }

        public ITransitionBuilder<State> AddTransitionOn(Event evt)
        {
             return this.innerState.AddTransitionOn(this, evt);
        }

        public ITransitionBuilder<State, TEventData> AddTransitionOn<TEventData>(Event<TEventData> evt)
        {
            return this.innerState.AddTransitionOn<TEventData>(this, evt);
        }

        public Transition<State> AddInnerSelfTransitionOn(Event evt)
        {
            return this.innerState.AddInnerSelfTransitionOn(this, evt);
        }

        public Transition<State, TEventData> AddInnerSelfTransitionOn<TEventData>(Event<TEventData> evt)
        {
            return this.innerState.AddInnerSelfTransitionOn<TEventData>(this, evt);
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

        public StateMachine CreateChildStateMachine(string name)
        {
            this.ChildStateMachine = new StateMachine(name, this);
            return this.ChildStateMachine;
        }

        void IState<State>.FireOnEntry(StateHandlerInfo<State> info)
        {
            var onEntry = this.OnEntry;
            if (onEntry != null)
                onEntry(info);

            if (this.ChildStateMachine != null)
                this.ChildStateMachine.ForceTransition(info.To, this.ChildStateMachine.InitialState, this.ChildStateMachine.InitialState, info.Event);
        }

        void IState<State>.FireOnExit(StateHandlerInfo<State> info)
        {
            if (this.ChildStateMachine != null)
                this.ChildStateMachine.ForceTransition(this.ChildStateMachine.CurrentState, info.From, null, info.Event);

            var onExit = this.OnExit;
            if (onExit != null)
                onExit(info);
        }

        string IState.Name
        {
            get { return this.innerState.Name; }
        }

        IStateMachine<State> IState<State>.ChildStateMachine
        {
            get { return this.ChildStateMachine; }
        }

        void IState<State>.Reset()
        {
            if (this.ChildStateMachine != null)
                this.ChildStateMachine.Reset();
        }

        void IState<State>.AddTransition(ITransition<State> transition)
        {
            this.innerState.AddTransition(transition);
        }

        public override string ToString()
        {
            return String.Format("<State Name={0}>", this.Name);
        }
    }

    public class State<TStateData> : IState<State<TStateData>>
    {
        private readonly StateInner<State<TStateData>> innerState;

        public TStateData Data { get; set; }
        public StateHandler<TStateData> OnEntry { get; set; }
        public StateHandler<TStateData> OnExit { get; set; }

        public StateMachine<TStateData> ChildStateMachine { get; private set; }
        public StateMachine<TStateData> ParentStateMachine { get; private set; }
        public IReadOnlyList<ITransition<State<TStateData>>> Transitions { get { return this.innerState.Transitions; } }
        IStateMachine IState.ChildStateMachine { get { return this.ChildStateMachine; } }
        IStateMachine IState.ParentStateMachine { get { return this.ParentStateMachine; } }
        IReadOnlyList<ITransition<IState>> IState.Transitions { get { return this.innerState.Transitions; } }
        IStateMachine<State<TStateData>> IState<State<TStateData>>.ParentStateMachine { get { return this.ParentStateMachine; } }

        public string Name { get { return this.innerState.Name; } }

        internal State(string name, StateMachine<TStateData> parentStateMachine)
        {
            this.ParentStateMachine = parentStateMachine;
            this.innerState = new StateInner<State<TStateData>>(name, parentStateMachine);
        }

        public ITransitionBuilder<State<TStateData>> AddTransitionOn(Event evt)
        {
            return this.innerState.AddTransitionOn(this, evt);
        }

        public ITransitionBuilder<State<TStateData>, TEventData> AddTransitionOn<TEventData>(Event<TEventData> evt)
        {
            return this.innerState.AddTransitionOn<TEventData>(this, evt);
        }

        public Transition<State<TStateData>> AddInnerSelfTransitionOn(Event evt)
        {
            return this.innerState.AddInnerSelfTransitionOn(this, evt);
        }

        public Transition<State<TStateData>, TEventData> AddInnerSelfTransitionOn<TEventData>(Event<TEventData> evt)
        {
            return this.innerState.AddInnerSelfTransitionOn<TEventData>(this, evt);
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

        public StateMachine<TStateData> CreateChildStateMachine(string name)
        {
            this.ChildStateMachine = new StateMachine<TStateData>(name, this);
            return this.ChildStateMachine;
        }

        void IState<State<TStateData>>.FireOnEntry(StateHandlerInfo<State<TStateData>> info)
        {
            var onEntry = this.OnEntry;
            if (onEntry != null)
                onEntry(info);

            if (this.ChildStateMachine != null)
                this.ChildStateMachine.ForceTransition(info.To, this.ChildStateMachine.InitialState, this.ChildStateMachine.InitialState, info.Event);
        }

        void IState<State<TStateData>>.FireOnExit(StateHandlerInfo<State<TStateData>> info)
        {
            if (this.ChildStateMachine != null)
                this.ChildStateMachine.ForceTransition(this.ChildStateMachine.CurrentState, info.From, null, info.Event);

            var onExit = this.OnExit;
            if (onExit != null)
                onExit(info);
        }

        string IState.Name
        {
            get { return this.innerState.Name; }
        }

        IStateMachine<State<TStateData>> IState<State<TStateData>>.ChildStateMachine
        {
            get { return this.ChildStateMachine; }
        }

        void IState<State<TStateData>>.Reset()
        {
            if (this.ChildStateMachine != null)
                this.ChildStateMachine.Reset();
        }

        void IState<State<TStateData>>.AddTransition(ITransition<State<TStateData>> transition)
        {
            this.innerState.AddTransition(transition);
        }

        public override string ToString()
        {
            return String.Format("<State Name={0} Data={1}>", this.Name, this.Data);
        }
    }
}
