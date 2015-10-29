using System;
using System.Collections.Generic;

namespace StateMechanic
{
    /// <summary>
    /// A state machine, which may exist as a child state machine
    /// </summary>
    public class ChildStateMachine : IStateMachine<State>, IEventDelegate, IStateDelegate<State>
    {
        internal StateMachineInner<State> InnerStateMachine { get; }

        /// <summary>
        /// Gets the state which this state machine is currently in
        /// </summary>
        public State CurrentState => this.InnerStateMachine.CurrentState;

        /// <summary>
        /// If <see cref="CurrentState"/> has a child state machine, gets that child state machine's current state (recursively), otherwise gets <see cref="CurrentState"/>
        /// </summary>
        public State CurrentChildState => this.InnerStateMachine.CurrentChildState;

        /// <summary>
        /// Gets the initial state of this state machine
        /// </summary>
        public State InitialState => this.InnerStateMachine.InitialState;

        /// <summary>
        /// Gets the name given to this state machine when it was created
        /// </summary>
        public string Name => this.InnerStateMachine.Name;

        IState IStateMachine.CurrentState => this.InnerStateMachine.CurrentState;
        IState IStateMachine.CurrentChildState => this.InnerStateMachine.CurrentChildState;
        IState IStateMachine.InitialState => this.InnerStateMachine.InitialState;

        /// <summary>
        /// Gets a list of all states which are part of this state machine
        /// </summary>
        public IReadOnlyList<State> States => this.InnerStateMachine.States;

        IReadOnlyList<IState> IStateMachine.States => this.InnerStateMachine.States;

        internal ChildStateMachine(string name, StateMachineKernel<State> kernel, State parentState)
        {
            this.InnerStateMachine = new StateMachineInner<State>(name, kernel, this, parentState);
        }

        /// <summary>
        /// Create a new state and add it to this state machine
        /// </summary>
        /// <param name="name">Name given to the state</param>
        /// <returns>The new state</returns>
        public State CreateState(string name)
        {
            var state = new State(name, this);
            this.InnerStateMachine.AddState(state);
            return state;
        }

        /// <summary>
        /// Create the state which this state machine will be in when it first starts. This must be called exactly once per state machine
        /// </summary>
        /// <param name="name">Name given to the state</param>
        /// <returns>The new state</returns>
        public State CreateInitialState(string name)
        {
            var state = this.CreateState(name);
            this.InnerStateMachine.SetInitialState(state);
            return state;
        }

        /// <summary>
        /// Create an event which can be used on this state machine, or its children
        /// </summary>
        /// <param name="name">Name given to the event</param>
        /// <returns>The new event</returns>
        public Event CreateEvent(string name) => this.InnerStateMachine.CreateEvent(name);

        /// <summary>
        /// Create an event which has associated data which can be used on this state machine, or its children
        /// </summary>
        /// <param name="name">Name given to the event</param>
        /// <returns>The new event</returns>
        public Event<TEventData> CreateEvent<TEventData>(string name) => this.InnerStateMachine.CreateEvent<TEventData>(name);

        /// <summary>
        /// Force a transition to the given state, even though there may not be a valid configured transition to that state from the current state
        /// </summary>
        /// <remarks>Exit and entry handlers will be fired, but no transition handler will be fired</remarks>
        /// <param name="toState">State to transition to</param>
        /// <param name="event">Event pass to the exit/entry handlers</param>
        public void ForceTransition(State toState, IEvent @event)
        {
            if (toState == null)
                throw new ArgumentNullException(nameof(toState));
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            if (toState.ParentStateMachine != this)
                throw new InvalidStateTransitionException(this.CurrentState, toState);
            if (@event.ParentStateMachine != this && !this.IsChildOf(@event.ParentStateMachine))
                throw new InvalidEventTransitionException(this.CurrentState, @event);

            var transitionInvoker = new ForcedTransitionInvoker<State>(toState, @event, this.InnerStateMachine.Kernel);
            this.InnerStateMachine.ForceTransitionFromUser(transitionInvoker);
        }

        /// <summary>
        /// Determines whether this state machine is a child of another state machine
        /// </summary>
        /// <param name="parentStateMachine">State machine which may be a parent of this state machine</param>
        /// <returns>True if this state machine is a child of the given state machine</returns>
        public bool IsChildOf(IStateMachine parentStateMachine)
        {
            if (parentStateMachine == null)
                throw new ArgumentNullException(nameof(parentStateMachine));

            return this.InnerStateMachine.IsChildOf(parentStateMachine);
        }

        internal void ResetChildStateMachine()
        {
            this.InnerStateMachine.Reset();
        }

        bool IStateMachine<State>.RequestEventFire(ITransitionInvoker<State> transitionInvoker, bool overrideNoThrow)
        {
            return this.InnerStateMachine.RequestEventFire(transitionInvoker, overrideNoThrow);
        }

        void IStateMachine<State>.SetCurrentState(State state)
        {
            this.InnerStateMachine.SetCurrentState(state);
        }

        bool IEventDelegate.RequestEventFireFromEvent(Event @event, EventFireMethod eventFireMethod)
        {
            var transitionInvoker = new EventTransitionInvoker<State>(@event, eventFireMethod);
            return this.InnerStateMachine.RequestEventFireFromEvent(transitionInvoker);
        }

        bool IEventDelegate.RequestEventFireFromEvent<TEventData>(Event<TEventData> @event, TEventData eventData, EventFireMethod eventFireMethod)
        {
            var transitionInvoker = new EventTransitionInvoker<State, TEventData>(@event, eventFireMethod, eventData);
            return this.InnerStateMachine.RequestEventFireFromEvent(transitionInvoker);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object</returns>
        public override string ToString()
        {
            return $"<StateMachine Parent={this.Name} State={this.CurrentState?.Name ?? "None"}>";
        }
    }
}
