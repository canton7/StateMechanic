using System;
using System.Collections.Generic;

namespace StateMechanic
{
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
        public string Name => this.innerState.Name;

        /// <summary>
        /// Gets a value indicating whether this state's parent state machine is in this state
        /// </summary>
        public bool IsCurrent => this.ParentStateMachine.CurrentState == this;

        /// <summary>
        /// Gets the child state machine of this state, if any
        /// </summary>
        public ChildStateMachine<TStateData> ChildStateMachine { get; private set; }

        /// <summary>
        /// Gets the state machine to which this state belongs
        /// </summary>
        public ChildStateMachine<TStateData> ParentStateMachine { get; }

        /// <summary>
        /// Gets a list of transitions available from the current state
        /// </summary>
        public IReadOnlyList<ITransition<State<TStateData>>> Transitions => this.innerState.Transitions;

        /// <summary>
        /// Gets the list of groups which this state is a member of
        /// </summary>
        public IReadOnlyList<IStateGroup> Groups => this.innerState.Groups;

        IStateMachine IState.ChildStateMachine => this.ChildStateMachine;
        IStateMachine IState.ParentStateMachine => this.ParentStateMachine;
        IReadOnlyList<ITransition<IState>> IState.Transitions => this.innerState.Transitions;
        IStateMachine<State<TStateData>> IState<State<TStateData>>.ChildStateMachine => this.ChildStateMachine;
        IStateMachine<State<TStateData>> IState<State<TStateData>>.ParentStateMachine => this.ParentStateMachine;
        IReadOnlyList<IStateGroup<State<TStateData>>> IState<State<TStateData>>.Groups => this.innerState.Groups;

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
        /// <returns>The created state machine</returns>
        public ChildStateMachine<TStateData> CreateChildStateMachine()
        {
            this.ChildStateMachine = new ChildStateMachine<TStateData>(this.Name, this.ParentStateMachine.InnerStateMachine.Kernel, this);
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
            group.AddStateInternal(this);
        }

        /// <summary>
        /// Add this state to the given groups
        /// </summary>
        /// <param name="groups">Grousp to add this state to</param>
        public void AddToGroups(params StateGroup<TStateData>[] groups)
        {
            foreach (var group in groups)
            {
                this.AddToGroup(group);
            }
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
            this.ChildStateMachine?.ResetChildStateMachine();
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
            return $"<State Name={this.Name} Data={this.Data}>";
        }
    }
}
