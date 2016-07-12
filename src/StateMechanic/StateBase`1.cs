using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace StateMechanic
{
    /// <summary>
    /// A state, which can be transioned from/to, and which can represent the current state of a <see cref="StateMachine{TState}"/>
    /// </summary>
    public abstract class StateBase<TState> : IState where TState : StateBase<TState>, new()
    {
        private bool isInitialized;
        private readonly TState self;

        private readonly List<ITransition<TState>> transitions = new List<ITransition<TState>>();

        /// <summary>
        /// Gets a list of transitions available from the current state
        /// </summary>
        public IReadOnlyList<ITransition<TState>> Transitions { get; }

        private readonly List<StateGroup<TState>> groups = new List<StateGroup<TState>>();

        /// <summary>
        /// Gets the list of groups which this state is a member of
        /// </summary>
        public IReadOnlyList<StateGroup<TState>> Groups { get; }

        private string _name;

        /// <summary>
        /// Gets the name assigned to this state
        /// </summary>
        public string Name
        {
            get
            {
                this.CheckInitialized();
                return this._name;
            }
            protected set
            {
                this._name = value;
            }
        }

        /// <summary>
        /// Gets the indentifier for the state. Used for serializing/deserializing state machines. Can be overridden.
        /// </summary>
        public virtual string Identifier => this.Name;

        /// <summary>
        /// Gets a value indicating whether this state's parent state machine is in this state
        /// </summary>
        public bool IsCurrent => this.ParentStateMachine.CurrentState == this;

        /// <summary>
        /// Gets the child state machine of this state, if any
        /// </summary>
        public ChildStateMachine<TState> ChildStateMachine { get; private set; }

        private ChildStateMachine<TState> _parentStateMachine;

        /// <summary>
        /// Gets the state machine to which this state belongs
        /// </summary>
        public ChildStateMachine<TState> ParentStateMachine
        {
            get
            {
                this.CheckInitialized();
                return this._parentStateMachine;
            }
            set
            {
                this._parentStateMachine = value;
            }
        }

        IStateMachine IState.ChildStateMachine => this.ChildStateMachine;
        IStateMachine IState.ParentStateMachine => this.ParentStateMachine;
        IReadOnlyList<ITransition<IState>> IState.Transitions => this.Transitions;
        IReadOnlyList<IStateGroup> IState.Groups => this.Groups;

        /// <summary>
        /// Gets or sets the method called when the StateMachine enters this state
        /// </summary>
        public Action<StateHandlerInfo<TState>> EntryHandler { get; set; }

        /// <summary>
        /// Gets or sets the method called when the StateMachine exits this state
        /// </summary>
        public Action<StateHandlerInfo<TState>> ExitHandler { get; set; }

        /// <summary>
        /// YOU SHOULD NOT CALL THIS! This is invoked by <see cref="ChildStateMachine{TState}.CreateState(string)"/>
        /// </summary>
        public StateBase()
        {
            var self = this as TState;
            if (self == null)
                throw new ArgumentException("TState must be the type of subclass");

            this.self = self;
            this.Transitions = new ReadOnlyCollection<ITransition<TState>>(this.transitions);
            this.Groups = new ReadOnlyCollection<StateGroup<TState>>(this.groups);
        }

        internal void Initialize(string name, ChildStateMachine<TState> parentStateMachine)
        {
            this.isInitialized = true;
            // If they've subclassed this and set Name themselves, then provided 'null' in CreateState,
            // don't override what they set
            if (!String.IsNullOrWhiteSpace(name))
                this.Name = name;
            this.ParentStateMachine = parentStateMachine;
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

            return new TransitionBuilder<TState>(this.self, @event, this.ParentStateMachine.TopmostStateMachineInternal);
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

            return new TransitionBuilder<TState, TEventData>(this.self, @event, this.ParentStateMachine.TopmostStateMachineInternal);
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

            var transition = new Transition<TState>(this.self, @event, this.ParentStateMachine.TopmostStateMachineInternal);
            @event.AddTransition(this, transition, this.ParentStateMachine.TopmostStateMachineInternal);
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

            var transition = new Transition<TState, TEventData>(this.self, @event, this.ParentStateMachine.TopmostStateMachineInternal);
            @event.AddTransition(this, transition, this.ParentStateMachine.TopmostStateMachineInternal);
            this.transitions.Add(transition);
            return transition;
        }

        /// <summary>
        /// Ignore a particular event. Firing the event will succeed, but no transition will occur
        /// </summary>
        /// <param name="event">Event to ignore</param>
        public void Ignore(Event @event)
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            var transition = new IgnoredTransition<TState>(this.self, @event, this.ParentStateMachine.TopmostStateMachineInternal);
            @event.AddTransition(this, transition, this.ParentStateMachine.TopmostStateMachineInternal);
        }

        /// <summary>
        /// Ignore a particular event. Firing the event will succeed, but no transition will occur
        /// </summary>
        /// <typeparam name="TEventData">Type of event data</typeparam>
        /// <param name="event">Event to ignore</param>
        public void Ignore<TEventData>(Event<TEventData> @event)
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            var transition = new IgnoredTransition<TState, TEventData>(this.self, @event, this.ParentStateMachine.TopmostStateMachineInternal);
            @event.AddTransition(this, transition, this.ParentStateMachine.TopmostStateMachineInternal);
        }

        /// <summary>
        /// Ignore multiple events. Firing these events will succeed, but no transition will occur
        /// </summary>
        /// <param name="events">Events to ignore</param>
        public void Ignore(params Event[] events)
        {
            if (events == null)
                throw new ArgumentNullException(nameof(events));

            foreach (var @event in events)
            {
                this.Ignore(@event);
            }
        }

        /// <summary>
        /// Ignore multiple events. Firing these events will succeed, but no transition will occur
        /// </summary>
        /// <typeparam name="TEventData">Type of event data</typeparam>
        /// <param name="events">Events to ignore</param>
        public void Ignore<TEventData>(params Event<TEventData>[] events)
        {
            if (events == null)
                throw new ArgumentNullException(nameof(events));

            foreach (var @event in events)
            {
                this.Ignore(@event);
            }
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

            this.ChildStateMachine = new ChildStateMachine<TState>(name ?? this.Name, this.self);
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

            if (!this.groups.Contains(group))
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
        protected internal virtual void OnEntry(StateHandlerInfo<TState> info)
        {
            this.EntryHandler?.Invoke(info);
        }

        /// <summary>
        /// Invoke the exit handler - override for custom behaviour
        /// </summary>
        /// <param name="info">Information associated with this transition</param>
        protected internal virtual void OnExit(StateHandlerInfo<TState> info)
        {
            this.ExitHandler?.Invoke(info);
        }

        /// <summary>
        /// Optional override point. If overridden, will be called each time an event is fired, and will return
        /// the state to transition to (will override whatever was configured in the transition), or null to
        /// use whatever was configured in the transitions.
        /// </summary>
        /// <param name="event">Event that was fired</param>
        /// <returns>State to transition to, or null</returns>
        protected internal virtual TState HandleEvent(IEvent @event)
        {
            return null;
        }


        /// <summary>
        /// Optional override point. If overridden, will be called before a transition occurs, and can return
        /// false to abort the transition. If it returns true, the transition guard will be called.
        /// </summary>
        /// <param name="event">Event which triggered the transition</param>
        /// <param name="to">State to transition to</param>
        /// <returns>False to abort the transition, true to continue</returns>
        protected internal virtual bool CanTransition(IEvent @event, TState to)
        {
            return true;
        }

        internal void Reset()
        {
            this.ChildStateMachine?.ResetChildStateMachine();
        }

        internal void AddTransition(ITransition<TState> transition)
        {
            this.transitions.Add(transition);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object</returns>
        [ExcludeFromCoverage]
        public override string ToString()
        {
            this.CheckInitialized();
            return $"<State Name={this.Name ?? "(unnamed)"}>";
        }
    }
}
