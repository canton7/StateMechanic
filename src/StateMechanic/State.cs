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
        public IReadOnlyList<ITransition<TState>> Transitions { get { return this.transitions; } }

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

        public StateHandler EntryHandler { get; set; }
        public StateHandler ExitHandler { get; set; }

        public string Name { get { return this.innerState.Name; } }
        public ChildStateMachine ChildStateMachine { get; private set; }
        public ChildStateMachine ParentStateMachine { get; private set; }
        public IReadOnlyList<ITransition<State>> Transitions { get { return this.innerState.Transitions; } }
        IStateMachine IState.ChildStateMachine { get { return this.ChildStateMachine; } }
        IStateMachine IState.ParentStateMachine { get { return this.ParentStateMachine; } }
        IReadOnlyList<ITransition<IState>> IState.Transitions { get { return this.innerState.Transitions; } }
        IStateMachine<State> IState<State>.ParentStateMachine { get { return this.ParentStateMachine; } }

        internal State(string name, ChildStateMachine parentStateMachine)
        {
            this.ParentStateMachine = parentStateMachine;
            this.innerState = new StateInner<State>(name, parentStateMachine);
        }

        public ITransitionBuilder<State> AddTransitionOn(Event @event)
        {
            if (@event == null)
                throw new ArgumentNullException("event");
            return this.innerState.AddTransitionOn(this, @event);
        }

        public ITransitionBuilder<State, TEventData> AddTransitionOn<TEventData>(Event<TEventData> @event)
        {
            if (@event == null)
                throw new ArgumentNullException("event");
            return this.innerState.AddTransitionOn<TEventData>(this, @event);
        }

        public Transition<State> AddInnerSelfTransitionOn(Event @event)
        {
            if (@event == null)
                throw new ArgumentNullException("event");
            return this.innerState.AddInnerSelfTransitionOn(this, @event);
        }

        public Transition<State, TEventData> AddInnerSelfTransitionOn<TEventData>(Event<TEventData> @event)
        {
            if (@event == null)
                throw new ArgumentNullException("event");
            return this.innerState.AddInnerSelfTransitionOn<TEventData>(this, @event);
        }

        public State WithEntry(StateHandler entryHandler)
        {
            this.EntryHandler = entryHandler;
            return this;
        }

        public State WithExit(StateHandler exitHandler)
        {
            this.ExitHandler = exitHandler;
            return this;
        }

        public ChildStateMachine CreateChildStateMachine(string name)
        {
            this.ChildStateMachine = new ChildStateMachine(name, this.ParentStateMachine.InnerStateMachine.Kernel, this);
            return this.ChildStateMachine;
        }

        void IState<State>.FireEntryHandler(StateHandlerInfo<State> info)
        {
            var entryHandler = this.EntryHandler;
            if (entryHandler != null)
                entryHandler(info);

            if (this.ChildStateMachine != null)
                this.ChildStateMachine.ForceTransition(info.To, this.ChildStateMachine.InitialState, this.ChildStateMachine.InitialState, info.Event);
        }

        void IState<State>.FireExitHandler(StateHandlerInfo<State> info)
        {
            if (this.ChildStateMachine != null)
                this.ChildStateMachine.ForceTransition(this.ChildStateMachine.CurrentState, info.From, null, info.Event);

            var exitHandler = this.ExitHandler;
            if (exitHandler != null)
                exitHandler(info);
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
                this.ChildStateMachine.ResetSubStateMachine();
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
        public StateHandler<TStateData> EntryHandler { get; set; }
        public StateHandler<TStateData> ExitHandler { get; set; }

        public ChildStateMachine<TStateData> ChildStateMachine { get; private set; }
        public ChildStateMachine<TStateData> ParentStateMachine { get; private set; }
        public IReadOnlyList<ITransition<State<TStateData>>> Transitions { get { return this.innerState.Transitions; } }
        IStateMachine IState.ChildStateMachine { get { return this.ChildStateMachine; } }
        IStateMachine IState.ParentStateMachine { get { return this.ParentStateMachine; } }
        IReadOnlyList<ITransition<IState>> IState.Transitions { get { return this.innerState.Transitions; } }
        IStateMachine<State<TStateData>> IState<State<TStateData>>.ParentStateMachine { get { return this.ParentStateMachine; } }

        public string Name { get { return this.innerState.Name; } }

        internal State(string name, TStateData data, ChildStateMachine<TStateData> parentStateMachine)
        {
            this.Data = data;
            this.ParentStateMachine = parentStateMachine;
            this.innerState = new StateInner<State<TStateData>>(name, parentStateMachine);
        }

        public ITransitionBuilder<State<TStateData>> AddTransitionOn(Event @event)
        {
            if (@event == null)
                throw new ArgumentNullException("event");
            return this.innerState.AddTransitionOn(this, @event);
        }

        public ITransitionBuilder<State<TStateData>, TEventData> AddTransitionOn<TEventData>(Event<TEventData> @event)
        {
            if (@event == null)
                throw new ArgumentNullException("event");
            return this.innerState.AddTransitionOn<TEventData>(this, @event);
        }

        public Transition<State<TStateData>> AddInnerSelfTransitionOn(Event @event)
        {
            if (@event == null)
                throw new ArgumentNullException("event");
            return this.innerState.AddInnerSelfTransitionOn(this, @event);
        }

        public Transition<State<TStateData>, TEventData> AddInnerSelfTransitionOn<TEventData>(Event<TEventData> @event)
        {
            if (@event == null)
                throw new ArgumentNullException("event");
            return this.innerState.AddInnerSelfTransitionOn<TEventData>(this, @event);
        }

        public State<TStateData> WithEntry(StateHandler<TStateData> entryHandler)
        {
            this.EntryHandler = entryHandler;
            return this;
        }

        public State<TStateData> WithExit(StateHandler<TStateData> exitHandler)
        {
            this.ExitHandler = exitHandler;
            return this;
        }

        public ChildStateMachine<TStateData> CreateChildStateMachine(string name)
        {
            this.ChildStateMachine = new ChildStateMachine<TStateData>(name, this.ParentStateMachine.InnerStateMachine.Kernel, this);
            return this.ChildStateMachine;
        }

        void IState<State<TStateData>>.FireEntryHandler(StateHandlerInfo<State<TStateData>> info)
        {
            var entryHandler = this.EntryHandler;
            if (entryHandler != null)
                entryHandler(info);

            if (this.ChildStateMachine != null)
                this.ChildStateMachine.ForceTransition(info.To, this.ChildStateMachine.InitialState, this.ChildStateMachine.InitialState, info.Event);
        }

        void IState<State<TStateData>>.FireExitHandler(StateHandlerInfo<State<TStateData>> info)
        {
            if (this.ChildStateMachine != null)
                this.ChildStateMachine.ForceTransition(this.ChildStateMachine.CurrentState, info.From, null, info.Event);

            var exitHandler = this.ExitHandler;
            if (exitHandler != null)
                exitHandler(info);
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
                this.ChildStateMachine.ResetSubStateMachine();
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
