using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace StateMechanic
{
    /// <summary>
    /// A state, which can be transioned from/to, and which can represent the current state of a <see cref="StateMachine"/>
    /// </summary>
    public class State : IState<State>
    {
        private readonly StateInner<State> innerState;

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
        public ChildStateMachine ChildStateMachine { get; private set; }

        /// <summary>
        /// Gets the state machine to which this state belongs
        /// </summary>
        public ChildStateMachine ParentStateMachine { get; }

        /// <summary>
        /// Gets a list of transitions available from the current state
        /// </summary>
        public IReadOnlyList<ITransition<State>> Transitions => this.innerState.Transitions;

        /// <summary>
        /// Gets the list of groups which this state is a member of
        /// </summary>
        public IReadOnlyList<IStateGroup> Groups => this.innerState.Groups;

        IStateMachine IState.ChildStateMachine => this.ChildStateMachine;
        IStateMachine IState.ParentStateMachine => this.ParentStateMachine;
        IReadOnlyList<ITransition<IState>> IState.Transitions => this.innerState.Transitions;
        IStateMachine<State> IState<State>.ChildStateMachine => this.ChildStateMachine;
        IStateMachine<State> IState<State>.ParentStateMachine => this.ParentStateMachine;
        IReadOnlyList<IStateGroup<State>> IState<State>.Groups => this.innerState.Groups;

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
        /// <returns>The created state machine</returns>
        public ChildStateMachine CreateChildStateMachine()
        {
            this.ChildStateMachine = new ChildStateMachine(this.Name, this.ParentStateMachine.InnerStateMachine.Kernel, this);
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
            this.ChildStateMachine?.ResetChildStateMachine();
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
            return $"<State Name={this.Name}>";
        }
    }
}
