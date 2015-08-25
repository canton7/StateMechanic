using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace StateMechanic
{
    internal class StateInner<TState> where TState : class, IState<TState>
    {
        private readonly ITransitionDelegate<TState> transitionDelegate;

        public IStateDelegate<TState> ChildStateMachine { get; set; }

        public string Name { get; private set; }

        private readonly List<ITransition<TState>> transitions = new List<ITransition<TState>>();
        public IReadOnlyList<ITransition<TState>> Transitions { get; private set; }

        private readonly List<IStateGroup<TState>> groups = new List<IStateGroup<TState>>();
        public IReadOnlyList<IStateGroup<TState>> Groups { get; private set; }

        public Action<StateHandlerInfo<TState>> EntryHandler { get; set; }
        public Action<StateHandlerInfo<TState>> ExitHandler { get; set; }

        internal StateInner(string name, ITransitionDelegate<TState> transitionDelegate)
        {
            this.Name = name;
            this.transitionDelegate = transitionDelegate;
            this.Transitions = new ReadOnlyCollection<ITransition<TState>>(this.transitions);
            this.Groups = new ReadOnlyCollection<IStateGroup<TState>>(this.groups);
        }

        public ITransitionBuilder<TState> TransitionOn(TState fromState, Event @event)
        {
            return new TransitionBuilder<TState>(fromState, @event, this.transitionDelegate);
        }

        public ITransitionBuilder<TState, TEventData> TransitionOn<TEventData>(TState fromState, Event<TEventData> @event)
        {
            return new TransitionBuilder<TState, TEventData>(fromState, @event, this.transitionDelegate);
        }

        public Transition<TState> InnerSelfTransitionOn(TState fromAndToState, Event @event)
        {
            var transition = Transition.CreateInner<TState>(fromAndToState, @event, this.transitionDelegate);
            @event.AddTransition(fromAndToState, transition);
            this.AddTransition(transition);
            return transition;
        }

        public Transition<TState, TEventData> InnerSelfTransitionOn<TEventData>(TState fromAndToState, Event<TEventData> @event)
        {
            var transition = Transition.CreateInner<TState, TEventData>(fromAndToState, @event, this.transitionDelegate);
            @event.AddTransition(fromAndToState, transition);
            this.AddTransition(transition);
            return transition;
        }

        public void AddTransition(ITransition<TState> transition)
        {
            this.transitions.Add(transition);
        }

        public void FireEntryHandler(StateHandlerInfo<TState> info)
        {
            var entryHandler = this.EntryHandler;
            if (entryHandler != null)
                entryHandler(info);
        }

        public void FireExitHandler(StateHandlerInfo<TState> info)
        {
            var exitHandler = this.ExitHandler;
            if (exitHandler != null)
                exitHandler(info);
        }

        public void AddGroup(IStateGroup<TState> group)
        {
            this.groups.Add(group);
        }
    }

    /// <summary>
    /// A state, which can be transioned from/to, and which can represent the current state of a <see cref="StateMachine"/>
    /// </summary>
    public class State : IState<State>
    {
        private readonly StateInner<State> innerState;

        /// <summary>
        /// Gets the name assigned to this state
        /// </summary>
        public string Name { get { return this.innerState.Name; } }

        /// <summary>
        /// Gets a value indicating whether this state's parent state machine is in this state
        /// </summary>
        public bool IsCurrent {  get { return this.ParentStateMachine.CurrentState == this; } }

        /// <summary>
        /// Gets the child state machine of this state, if any
        /// </summary>
        public ChildStateMachine ChildStateMachine { get; private set; }

        /// <summary>
        /// Gets the state machine to which this state belongs
        /// </summary>
        public ChildStateMachine ParentStateMachine { get; private set; }

        /// <summary>
        /// Gets a list of transitions available from the current state
        /// </summary>
        public IReadOnlyList<ITransition<State>> Transitions { get { return this.innerState.Transitions; } }

        /// <summary>
        /// Gets the list of groups which this state is a member of
        /// </summary>
        public IReadOnlyList<IStateGroup> Groups { get { return this.innerState.Groups; } }

        IStateMachine IState.ChildStateMachine { get { return this.ChildStateMachine; } }
        IStateMachine IState.ParentStateMachine { get { return this.ParentStateMachine; } }
        IReadOnlyList<ITransition<IState>> IState.Transitions { get { return this.innerState.Transitions; } }
        IStateMachine<State> IState<State>.ChildStateMachine { get { return this.ChildStateMachine; } }
        IStateMachine<State> IState<State>.ParentStateMachine { get { return this.ParentStateMachine; } }
        IReadOnlyList<IStateGroup<State>> IState<State>.Groups {  get { return this.innerState.Groups; } }

        /// <summary>
        /// Gets or sets the method called when the StateMachine enters this state
        /// </summary>
        public Action<StateHandlerInfo<State>> EntryHandler
        {
            get { return this.innerState.EntryHandler; }
            set { this.innerState.EntryHandler = value; }
        }

        /// <summary>
        /// Gets or sets the method called when the StateMachine exits this state
        /// </summary>
        public Action<StateHandlerInfo<State>> ExitHandler
        {
            get { return this.innerState.ExitHandler; }
            set { this.innerState.ExitHandler = value; }
        }

        internal State(string name, ChildStateMachine parentStateMachine)
        {
            this.ParentStateMachine = parentStateMachine;
            this.innerState = new StateInner<State>(name, parentStateMachine.InnerStateMachine.Kernel);
        }

        /// <summary>
        /// Create a transition on an event to some other state
        /// </summary>
        /// <param name="event">Event which will trigger this transition</param>
        /// <returns>An <see cref="ITransitionBuilder{State}"/> which can be used to finish setting up the transition</returns>
        public ITransitionBuilder<State> TransitionOn(Event @event)
        {
            if (@event == null)
                throw new ArgumentNullException("event");
            return this.innerState.TransitionOn(this, @event);
        }

        /// <summary>
        /// Create a transition on an event to some other state
        /// </summary>
        /// <param name="event">Event which will trigger this transition</param>
        /// <returns>An <see cref="ITransitionBuilder{State, TEventData}"/> which can be used to finish setting up the transition</returns>
        public ITransitionBuilder<State, TEventData> TransitionOn<TEventData>(Event<TEventData> @event)
        {
            if (@event == null)
                throw new ArgumentNullException("event");
            return this.innerState.TransitionOn<TEventData>(this, @event);
        }

        /// <summary>
        /// Create an inner self transition, i.e. a transition back to this state which will not call this state's exit/entry handlers
        /// </summary>
        /// <param name="event">Event which will trigger this transition</param>
        /// <returns>The created transition, to which handlers can be addeds</returns>
        public Transition<State> InnerSelfTransitionOn(Event @event)
        {
            if (@event == null)
                throw new ArgumentNullException("event");
            return this.innerState.InnerSelfTransitionOn(this, @event);
        }

        /// <summary>
        /// Create an inner self transition, i.e. a transition back to this state which will not call this state's exit/entry handlers
        /// </summary>
        /// <param name="event">Event which will trigger this transition</param>
        /// <returns>The created transition, to which handlers can be addeds</returns>
        public Transition<State, TEventData> InnerSelfTransitionOn<TEventData>(Event<TEventData> @event)
        {
            if (@event == null)
                throw new ArgumentNullException("event");
            return this.innerState.InnerSelfTransitionOn<TEventData>(this, @event);
        }

        /// <summary>
        /// Set the method called when the StateMachine enters this state
        /// </summary>
        /// <param name="entryHandler">Method called when the StateMachine enters this state</param>
        /// <returns>This State, used for method chaining</returns>
        public State WithEntry(Action<StateHandlerInfo<State>> entryHandler)
        {
            this.EntryHandler = entryHandler;
            return this;
        }

        /// <summary>
        /// Set the method called when the StateMachine exits this state
        /// </summary>
        /// <param name="exitHandler">Method called when the StateMachine exits this state</param>
        /// <returns>This State, used for method chaining</returns>
        public State WithExit(Action<StateHandlerInfo<State>> exitHandler)
        {
            this.ExitHandler = exitHandler;
            return this;
        }

        /// <summary>
        /// Create a StateMachine belonging to this state, which will be started when this State is entered
        /// </summary>
        /// <param name="name">Name of the state machine</param>
        /// <returns>The created state machine</returns>
        public ChildStateMachine CreateChildStateMachine(string name)
        {
            this.ChildStateMachine = new ChildStateMachine(name, this.ParentStateMachine.InnerStateMachine.Kernel, this);
            this.innerState.ChildStateMachine = this.ChildStateMachine;
            return this.ChildStateMachine;
        }

        /// <summary>
        /// Add this state to the given group
        /// </summary>
        /// <param name="group">Group to add this state to</param>
        public void AddToGroup(StateGroup group)
        {
            this.innerState.AddGroup(group);
            group.AddState(this);
        }

        void IState<State>.FireEntryHandler(StateHandlerInfo<State> info)
        {
            this.innerState.FireEntryHandler(info);
        }

        void IState<State>.FireExitHandler(StateHandlerInfo<State> info)
        {
            this.innerState.FireExitHandler(info);
        }

        void IState<State>.Reset()
        {
            if (this.ChildStateMachine != null)
                this.ChildStateMachine.ResetChildStateMachine();
        }

        void IState<State>.AddTransition(ITransition<State> transition)
        {
            this.innerState.AddTransition(transition);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object</returns>
        public override string ToString()
        {
            return String.Format("<State Name={0}>", this.Name);
        }
    }

    /// <summary>
    /// A state, which can be transioned from/to, and which can represent the current state of a <see cref="StateMachine"/>
    /// </summary>
    /// <typeparam name="TStateData">Type of data associated with this state</typeparam>
    public class State<TStateData> : IState<State<TStateData>>
    {
        private readonly StateInner<State<TStateData>> innerState;

        /// <summary>
        /// Gets or sets the data associated with this state
        /// </summary>
        public TStateData Data { get; set; }

        /// <summary>
        /// Gets the name assigned to this state
        /// </summary>
        public string Name { get { return this.innerState.Name; } }

        /// <summary>
        /// Gets a value indicating whether this state's parent state machine is in this state
        /// </summary>
        public bool IsCurrent { get { return this.ParentStateMachine.CurrentState == this; } }

        /// <summary>
        /// Gets the child state machine of this state, if any
        /// </summary>
        public ChildStateMachine<TStateData> ChildStateMachine { get; private set; }

        /// <summary>
        /// Gets the state machine to which this state belongs
        /// </summary>
        public ChildStateMachine<TStateData> ParentStateMachine { get; private set; }

        /// <summary>
        /// Gets a list of transitions available from the current state
        /// </summary>
        public IReadOnlyList<ITransition<State<TStateData>>> Transitions { get { return this.innerState.Transitions; } }

        /// <summary>
        /// Gets the list of groups which this state is a member of
        /// </summary>
        public IReadOnlyList<IStateGroup> Groups { get { return this.innerState.Groups; } }

        IStateMachine IState.ChildStateMachine { get { return this.ChildStateMachine; } }
        IStateMachine IState.ParentStateMachine { get { return this.ParentStateMachine; } }
        IReadOnlyList<ITransition<IState>> IState.Transitions { get { return this.innerState.Transitions; } }
        IStateMachine<State<TStateData>> IState<State<TStateData>>.ChildStateMachine { get { return this.ChildStateMachine; } }
        IStateMachine<State<TStateData>> IState<State<TStateData>>.ParentStateMachine { get { return this.ParentStateMachine; } }
        IReadOnlyList<IStateGroup<State<TStateData>>> IState<State<TStateData>>.Groups { get { return this.innerState.Groups; } }

        /// <summary>
        /// Gets or sets the method called when the StateMachine enters this state
        /// </summary>
        public Action<StateHandlerInfo<State<TStateData>>> EntryHandler
        {
            get { return this.innerState.EntryHandler; }
            set { this.innerState.EntryHandler = value; }
        }

        /// <summary>
        /// Gets or sets the method called when the StateMachine exits this state
        /// </summary>
        public Action<StateHandlerInfo<State<TStateData>>> ExitHandler
        {
            get { return this.innerState.ExitHandler; }
            set { this.innerState.ExitHandler = value; }
        }

        internal State(string name, TStateData data, ChildStateMachine<TStateData> parentStateMachine)
        {
            this.Data = data;
            this.ParentStateMachine = parentStateMachine;
            this.innerState = new StateInner<State<TStateData>>(name, parentStateMachine.InnerStateMachine.Kernel);
        }

        /// <summary>
        /// Create a transition on an event to some other state
        /// </summary>
        /// <param name="event">Event which will trigger this transition</param>
        /// <returns>An <see cref="ITransitionBuilder{TState}"/> which can be used to finish setting up the transition</returns>
        public ITransitionBuilder<State<TStateData>> TransitionOn(Event @event)
        {
            if (@event == null)
                throw new ArgumentNullException("event");
            return this.innerState.TransitionOn(this, @event);
        }

        /// <summary>
        /// Create a transition on an event to some other state
        /// </summary>
        /// <param name="event">Event which will trigger this transition</param>
        /// <returns>An <see cref="ITransitionBuilder{TState, TEventData}"/> which can be used to finish setting up the transition</returns>
        public ITransitionBuilder<State<TStateData>, TEventData> TransitionOn<TEventData>(Event<TEventData> @event)
        {
            if (@event == null)
                throw new ArgumentNullException("event");
            return this.innerState.TransitionOn<TEventData>(this, @event);
        }

        /// <summary>
        /// Create an inner self transition, i.e. a transition back to this state which will not call this state's exit/entry handlers
        /// </summary>
        /// <param name="event">Event which will trigger this transition</param>
        /// <returns>The created transition, to which handlers can be addeds</returns>
        public Transition<State<TStateData>> InnerSelfTransitionOn(Event @event)
        {
            if (@event == null)
                throw new ArgumentNullException("event");
            return this.innerState.InnerSelfTransitionOn(this, @event);
        }

        /// <summary>
        /// Create an inner self transition, i.e. a transition back to this state which will not call this state's exit/entry handlers
        /// </summary>
        /// <param name="event">Event which will trigger this transition</param>
        /// <returns>The created transition, to which handlers can be addeds</returns>
        public Transition<State<TStateData>, TEventData> InnerSelfTransitionOn<TEventData>(Event<TEventData> @event)
        {
            if (@event == null)
                throw new ArgumentNullException("event");
            return this.innerState.InnerSelfTransitionOn<TEventData>(this, @event);
        }

        /// <summary>
        /// Set the method called when the StateMachine enters this state
        /// </summary>
        /// <param name="entryHandler">Method called when the StateMachine enters this state</param>
        /// <returns>This State, used for method chaining</returns>
        public State<TStateData> WithEntry(Action<StateHandlerInfo<State<TStateData>>> entryHandler)
        {
            this.EntryHandler = entryHandler;
            return this;
        }

        /// <summary>
        /// Set the method called when the StateMachine exits this state
        /// </summary>
        /// <param name="exitHandler">Method called when the StateMachine exits this state</param>
        /// <returns>This State, used for method chaining</returns>
        public State<TStateData> WithExit(Action<StateHandlerInfo<State<TStateData>>> exitHandler)
        {
            this.ExitHandler = exitHandler;
            return this;
        }

        /// <summary>
        /// Create a StateMachine belonging to this state, which will be started when this State is entered
        /// </summary>
        /// <param name="name">Name of the state machine</param>
        /// <returns>The created state machine</returns>
        public ChildStateMachine<TStateData> CreateChildStateMachine(string name)
        {
            this.ChildStateMachine = new ChildStateMachine<TStateData>(name, this.ParentStateMachine.InnerStateMachine.Kernel, this);
            this.innerState.ChildStateMachine = this.ChildStateMachine;
            return this.ChildStateMachine;
        }

        /// <summary>
        /// Add this state to the given group
        /// </summary>
        /// <param name="group">Group to add this state to</param>
        public void AddToGroup(StateGroup<TStateData> group)
        {
            this.innerState.AddGroup(group);
            group.AddState(this);
        }

        void IState<State<TStateData>>.FireEntryHandler(StateHandlerInfo<State<TStateData>> info)
        {
            this.innerState.FireEntryHandler(info);
        }

        void IState<State<TStateData>>.FireExitHandler(StateHandlerInfo<State<TStateData>> info)
        {
            this.innerState.FireExitHandler(info);
        }

        void IState<State<TStateData>>.Reset()
        {
            if (this.ChildStateMachine != null)
                this.ChildStateMachine.ResetChildStateMachine();
        }

        void IState<State<TStateData>>.AddTransition(ITransition<State<TStateData>> transition)
        {
            this.innerState.AddTransition(transition);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object</returns>
        public override string ToString()
        {
            return String.Format("<State Name={0} Data={1}>", this.Name, this.Data);
        }
    }
}
