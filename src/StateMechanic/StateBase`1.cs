using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace StateMechanic
{
    /// <summary>
    /// A state, which can be transioned from/to, and which can represent the current state of a <see cref="StateMachine{TState}"/>
    /// </summary>
    public abstract class StateBase<TState> : IState<TState> where TState : StateBase<TState>, new()
    {
        private bool isInitialized;
        private readonly TState self;

        private readonly List<ITransition<TState>> transitions = new List<ITransition<TState>>();

        /// <summary>
        /// Gets a list of transitions available from the current state
        /// </summary>
        public IReadOnlyList<ITransition<TState>> Transitions { get; }

        private readonly List<IStateGroup<TState>> groups = new List<IStateGroup<TState>>();
        private readonly IReadOnlyList<IStateGroup<TState>> readOnlyGroups;

        internal ChildStateMachine<TState> ParentStateMachineInternal { get; private set; }

        /// <summary>
        /// Gets the list of groups which this state is a member of
        /// </summary>
        public IReadOnlyList<IStateGroup> Groups => this.readOnlyGroups;

        /// <summary>
        /// Gets the name assigned to this state
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this state's parent state machine is in this state
        /// </summary>
        public bool IsCurrent => this.ParentStateMachineInternal.CurrentState == this;

        /// <summary>
        /// Gets the child state machine of this state, if any
        /// </summary>
        public ChildStateMachine<TState> ChildStateMachine { get; private set; }

        /// <summary>
        /// Gets the state machine to which this state belongs
        /// </summary>
        public IStateMachine ParentStateMachine => this.ParentStateMachineInternal;

        IStateMachine IState.ChildStateMachine => this.ChildStateMachine;
        IStateMachine IState.ParentStateMachine => this.ParentStateMachineInternal;
        IReadOnlyList<ITransition<IState>> IState.Transitions => this.Transitions;
        IStateMachine<TState> IState<TState>.ChildStateMachine => this.ChildStateMachine;
        IStateMachine<TState> IState<TState>.ParentStateMachine => this.ParentStateMachineInternal;
        IReadOnlyList<IStateGroup<TState>> IState<TState>.Groups => this.readOnlyGroups;

        /// <summary>
        /// Gets or sets the method called when the StateMachine enters this state
        /// </summary>
        public Action<StateHandlerInfo<TState>> EntryHandler { get; set; }

        /// <summary>
        /// Gets or sets the method called when the StateMachine exits this state
        /// </summary>
        public Action<StateHandlerInfo<TState>> ExitHandler { get; set; }

        internal StateBase()
        {
            var self = this as TState;
            if (self == null)
                throw new ArgumentException("TState must be the type of subclass");

            this.self = self;
            this.Transitions = new ReadOnlyCollection<ITransition<TState>>(this.transitions);
            this.readOnlyGroups = new ReadOnlyCollection<IStateGroup<TState>>(this.groups);
        }

        internal void Initialize(string name, ChildStateMachine<TState> parentStateMachine)
        {
            this.isInitialized = true;
            this.Name = name;
            this.ParentStateMachineInternal = parentStateMachine;
        }

        private void CheckInitialized()
        {
            if (!this.isInitialized)
                throw new InvalidOperationException("You may not create states yourself. Use ChildStateMachine.CreateState (or StateMachine.CreateState) instead");
        }

        /// <summary>
        /// Create a transition on an event to some other state
        /// </summary>
        /// <param name="event">Event which will trigger this transition</param>
        /// <returns>An <see cref="ITransitionBuilder{State}"/> which can be used to finish setting up the transition</returns>
        public ITransitionBuilder<TState> TransitionOn(Event @event)
        {
            this.CheckInitialized();
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            return new TransitionBuilder<TState>(this.self, @event, this.ParentStateMachineInternal.Kernel);
        }

        /// <summary>
        /// Create a transition on an event to some other state
        /// </summary>
        /// <param name="event">Event which will trigger this transition</param>
        /// <returns>An <see cref="ITransitionBuilder{State, TEventData}"/> which can be used to finish setting up the transition</returns>
        public ITransitionBuilder<TState, TEventData> TransitionOn<TEventData>(Event<TEventData> @event)
        {
            this.CheckInitialized();
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            return new TransitionBuilder<TState, TEventData>(this.self, @event, this.ParentStateMachineInternal.Kernel);
        }

        /// <summary>
        /// Create an inner self transition, i.e. a transition back to this state which will not call this state's exit/entry handlers
        /// </summary>
        /// <param name="event">Event which will trigger this transition</param>
        /// <returns>The created transition, to which handlers can be addeds</returns>
        public Transition<TState> InnerSelfTransitionOn(Event @event)
        {
            this.CheckInitialized();
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            var transition = new Transition<TState>(this.self, @event, this.ParentStateMachineInternal.Kernel);
            @event.AddTransition(this, transition, this.ParentStateMachineInternal);
            this.transitions.Add(transition);
            return transition;
        }

        /// <summary>
        /// Create an inner self transition, i.e. a transition back to this state which will not call this state's exit/entry handlers
        /// </summary>
        /// <param name="event">Event which will trigger this transition</param>
        /// <returns>The created transition, to which handlers can be addeds</returns>
        public Transition<TState, TEventData> InnerSelfTransitionOn<TEventData>(Event<TEventData> @event)
        {
            this.CheckInitialized();
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            var transition = new Transition<TState, TEventData>(this.self, @event, this.ParentStateMachineInternal.Kernel);
            @event.AddTransition(this, transition, this.ParentStateMachineInternal);
            this.transitions.Add(transition);
            return transition;
        }

        /// <summary>
        /// Set the method called when the StateMachine enters this state
        /// </summary>
        /// <param name="entryHandler">Method called when the StateMachine enters this state</param>
        /// <returns>This State, used for method chaining</returns>
        public TState WithEntry(Action<StateHandlerInfo<TState>> entryHandler)
        {
            this.CheckInitialized();
            this.EntryHandler = entryHandler;
            return this.self;
        }

        /// <summary>
        /// Set the method called when the StateMachine exits this state
        /// </summary>
        /// <param name="exitHandler">Method called when the StateMachine exits this state</param>
        /// <returns>This State, used for method chaining</returns>
        public TState WithExit(Action<StateHandlerInfo<TState>> exitHandler)
        {
            this.CheckInitialized();
            this.ExitHandler = exitHandler;
            return this.self;
        }

        /// <summary>
        /// Create a StateMachine belonging to this state, which will be started when this State is entered
        /// </summary>
        /// <param name="name">Optional name (inherits name of child state if not set)</param>
        /// <returns>The created state machine</returns>
        public ChildStateMachine<TState> CreateChildStateMachine(string name = null)
        {
            this.CheckInitialized();
            if (this.ChildStateMachine != null)
                throw new InvalidOperationException("This state already has a child state machine");

            this.ChildStateMachine = new ChildStateMachine<TState>(name ?? this.Name, this.ParentStateMachineInternal.Kernel, this.self);
            return this.ChildStateMachine;
        }

        /// <summary>
        /// Add this state to the given group
        /// </summary>
        /// <param name="group">Group to add this state to</param>
        public void AddToGroup(StateGroup<TState> group)
        {
            this.CheckInitialized();
            if (group == null)
                throw new ArgumentNullException(nameof(group));

            this.groups.Add(group);
            group.AddStateInternal(this.self);
        }

        /// <summary>
        /// Add this state to the given groups
        /// </summary>
        /// <param name="groups">Grousp to add this state to</param>
        public void AddToGroups(params StateGroup<TState>[] groups)
        {
            this.CheckInitialized();
            if (groups == null)
                throw new ArgumentNullException(nameof(groups));

            foreach (var group in groups)
            {
                this.AddToGroup(group);
            }
        }

        /// <summary>
        /// Invoke the entry handler - override for custom behaviour
        /// </summary>
        /// <param name="info">Information associated with this transition</param>
        protected virtual void OnEntry(StateHandlerInfo<TState> info)
        {
            this.EntryHandler?.Invoke(info);
        }

        /// <summary>
        /// Invoke the exit handler - override for custom behaviour
        /// </summary>
        /// <param name="info">Information associated with this transition</param>
        protected virtual void OnExit(StateHandlerInfo<TState> info)
        {
            this.ExitHandler?.Invoke(info);
        }

        void IState<TState>.FireEntryHandler(StateHandlerInfo<TState> info)
        {
            this.OnEntry(info);
        }

        void IState<TState>.FireExitHandler(StateHandlerInfo<TState> info)
        {
            this.OnExit(info);
        }

        internal void Reset()
        {
            this.ChildStateMachine?.ResetChildStateMachine();
        }

        void IState<TState>.AddTransition(ITransition<TState> transition)
        {
            this.transitions.Add(transition);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object</returns>
        public override string ToString()
        {
            this.CheckInitialized();
            return $"<State Name={this.Name}>";
        }
    }
}
