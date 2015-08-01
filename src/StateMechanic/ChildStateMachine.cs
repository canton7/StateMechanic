using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    /// <summary>
    /// A state machine, which may exist as a child state machine
    /// </summary>
    public class ChildStateMachine : IStateMachine<State>, ITransitionDelegate<State>, IEventDelegate, IStateDelegate<State>
    {
        internal StateMachineInner<State> InnerStateMachine { get; private set; }

        /// <summary>
        /// Gets the state which this state machine is currently in
        /// </summary>
        public State CurrentState { get { return this.InnerStateMachine.CurrentState; } }

        /// <summary>
        /// If <see cref="CurrentState"/> has a child state machine, gets that child state machine's current state (recursively), otherwise gets <see cref="CurrentState"/>
        /// </summary>
        public State CurrentStateRecursive { get { return this.InnerStateMachine.CurrentStateRecursive; } }

        /// <summary>
        /// Gets the initial state of this state machine
        /// </summary>
        public State InitialState { get { return this.InnerStateMachine.InitialState; } }

        /// <summary>
        /// Gets the name given to this state machine when it was created
        /// </summary>
        public string Name { get { return this.InnerStateMachine.Name; } }

        /// <summary>
        /// Gets the state which this state machine is currently in
        /// </summary>
        IState IStateMachine.CurrentState { get { return this.InnerStateMachine.CurrentState; } }

        /// <summary>
        /// If <see cref="CurrentState"/> has a child state machine, gets that child state machine's current state (recursively), otherwise gets <see cref="CurrentState"/>
        /// </summary>
        IState IStateMachine.CurrentStateRecursive { get { return this.InnerStateMachine.CurrentStateRecursive; } }

        /// <summary>
        /// Gets the initial state of this state machine
        /// </summary>
        IState IStateMachine.InitialState { get { return this.InnerStateMachine.InitialState; } }

        /// <summary>
        /// Gets a list of all states which are part of this state machine
        /// </summary>
        public IReadOnlyList<State> States { get { return this.InnerStateMachine.States; } }

        /// <summary>
        /// Gets a list of all states which are part of this state machine
        /// </summary>
        IReadOnlyList<IState> IStateMachine.States { get { return this.InnerStateMachine.States; } }

        /// <summary>
        /// Event raised whenever a transition occurs on this state machine
        /// </summary>
        public event EventHandler<TransitionEventArgs<State>> Transition;

        /// <summary>
        /// Event raised whenever an event is fired but no corresponding transition is found on this state machine
        /// </summary>
        public event EventHandler<TransitionNotFoundEventArgs<State>> TransitionNotFound;

        internal ChildStateMachine(string name, StateMachineKernel<State> kernel, State parentState)
        {
            this.InnerStateMachine = new StateMachineInner<State>(name, kernel, this, parentState);
            this.InnerStateMachine.Transition += this.OnTransition;
            this.InnerStateMachine.TransitionNotFound += this.OnTransitionNotFound;
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
        public Event CreateEvent(string name)
        {
            return this.InnerStateMachine.CreateEvent(name);
        }

        /// <summary>
        /// Create an event which has associated data which can be used on this state machine, or its children
        /// </summary>
        /// <param name="name">Name given to the event</param>
        /// <returns>The new event</returns>
        public Event<TEventData> CreateEvent<TEventData>(string name)
        {
            return this.InnerStateMachine.CreateEvent<TEventData>(name);
        }

        /// <summary>
        /// Force a transition to the given state, even though there may not be a valid configured transition to that state from the current state
        /// </summary>
        /// <remarks>Exit and entry handlers will be fired, but no transition handler will be fired</remarks>
        /// <param name="toState">State to transition to</param>
        /// <param name="evt">Event pass to the exit/entry handlers</param>
        public void ForceTransition(State toState, IEvent evt)
        {
            if (toState == null)
                throw new ArgumentNullException("toState");
            if (evt == null)
                throw new ArgumentNullException("evt");

            if (toState.ParentStateMachine != this)
                throw new InvalidStateTransitionException(this.CurrentState, toState);
            if (evt.ParentStateMachine != this && !this.IsChildOf(evt.ParentStateMachine))
                throw new InvalidEventTransitionException(this.CurrentState, evt);

            this.InnerStateMachine.ForceTransition(this.CurrentState, toState, toState, evt);
        }

        /// <summary>
        /// Determines whether this state machine is a child of another state machine
        /// </summary>
        /// <param name="parentStateMachine">State machine which may be a parent of this state machine</param>
        /// <returns>True if this state machine is a child of the given state machine</returns>
        public bool IsChildOf(IStateMachine parentStateMachine)
        {
            if (parentStateMachine == null)
                throw new ArgumentNullException("parentStateMachine");

            return this.InnerStateMachine.IsChildOf(parentStateMachine);
        }

        /// <summary>
        /// Determines whether this state machine is in the given state, or the current state's child state machine is in the given state, recursively
        /// </summary>
        /// <param name="state">The state to test</param>
        /// <returns>True if this state machine is in the given state, or the current state's child state machine is in the given state, recursively</returns>
        public bool IsInStateRecursive(IState state)
        {
            if (state == null)
                throw new ArgumentNullException("state");

            return this.InnerStateMachine.IsInStateRecursive(state);
        }

        internal void ResetChildStateMachine()
        {
            this.InnerStateMachine.Reset();
        }

        void IStateDelegate<State>.ForceTransition(State pretendFromState, State pretendToState, State toState, IEvent evt)
        {
            this.InnerStateMachine.ForceTransition(pretendFromState, pretendToState, toState, evt);
        }

        void ITransitionDelegate<State>.UpdateCurrentState(State from, State state, IEvent evt, bool isInnerTransition)
        {
            this.InnerStateMachine.UpdateCurrentState(from, state, evt, isInnerTransition);
        }

        bool IStateMachine<State>.RequestEventFire(IEvent sourceEvent, Func<IState, bool> invoker, bool throwIfNotFound)
        {
            return this.InnerStateMachine.RequestEventFire(sourceEvent, invoker, throwIfNotFound);
        }

        bool IEventDelegate.RequestEventFireFromEvent(IEvent sourceEvent, Func<IState, bool> invoker, bool throwIfNotFound)
        {
            return this.InnerStateMachine.RequestEventFireFromEvent(sourceEvent, invoker, throwIfNotFound);
        }

        private void OnTransition(object sender, TransitionEventArgs<State> eventArgs)
        {
            var handler = this.Transition;
            if (handler != null)
                handler(this, eventArgs);
        }

        private void OnTransitionNotFound(object sender, TransitionNotFoundEventArgs<State> eventArgs)
        {
            var handler = this.TransitionNotFound;
            if (handler != null)
                handler(this, eventArgs);
        }
    }

    /// <summary>
    /// A state machine with per-state data, which may exist as a child state machine
    /// </summary>
    public class ChildStateMachine<TStateData> : IStateMachine<State<TStateData>>, ITransitionDelegate<State<TStateData>>, IEventDelegate, IStateDelegate<State<TStateData>>
    {
        internal StateMachineInner<State<TStateData>> InnerStateMachine { get; private set; }

        /// <summary>
        /// Gets the state which this state machine is currently in
        /// </summary>
        public State<TStateData> CurrentState { get { return this.InnerStateMachine.CurrentState; } }

        /// <summary>
        /// If <see cref="CurrentState"/> has a child state machine, gets that child state machine's current state (recursively), otherwise gets <see cref="CurrentState"/>
        /// </summary>
        public State<TStateData> CurrentStateRecursive { get { return this.InnerStateMachine.CurrentStateRecursive; } }

        /// <summary>
        /// Gets the initial state of this state machine
        /// </summary>
        public State<TStateData> InitialState { get { return this.InnerStateMachine.InitialState; } }

        /// <summary>
        /// Gets the name given to this state machine when it was created
        /// </summary>
        public string Name { get { return this.InnerStateMachine.Name; } }

        /// <summary>
        /// Gets the state which this state machine is currently in
        /// </summary>
        IState IStateMachine.CurrentState { get { return this.InnerStateMachine.CurrentState; } }

        /// <summary>
        /// If <see cref="CurrentState"/> has a child state machine, gets that child state machine's current state (recursively), otherwise gets <see cref="CurrentState"/>
        /// </summary>
        IState IStateMachine.CurrentStateRecursive { get { return this.InnerStateMachine.CurrentStateRecursive; } }

        /// <summary>
        /// Gets the initial state of this state machine
        /// </summary>
        IState IStateMachine.InitialState { get { return this.InnerStateMachine.InitialState; } }

        /// <summary>
        /// Gets a list of all states which are part of this state machine
        /// </summary>
        public IReadOnlyList<State<TStateData>> States { get { return this.InnerStateMachine.States; } }

        /// <summary>
        /// Gets a list of all states which are part of this state machine
        /// </summary>
        IReadOnlyList<IState> IStateMachine.States { get { return this.InnerStateMachine.States; } }

        /// <summary>
        /// Event raised whenever a transition occurs on this state machine
        /// </summary>
        public event EventHandler<TransitionEventArgs<State<TStateData>>> Transition;

        /// <summary>
        /// Event raised whenever an event is fired but no corresponding transition is found on this state machine
        /// </summary>
        public event EventHandler<TransitionNotFoundEventArgs<State<TStateData>>> TransitionNotFound;

        internal ChildStateMachine(string name, StateMachineKernel<State<TStateData>> kernel, State<TStateData> parentState)
        {
            this.InnerStateMachine = new StateMachineInner<State<TStateData>>(name, kernel, this, parentState);
            this.InnerStateMachine.Transition += this.OnTransition;
            this.InnerStateMachine.TransitionNotFound += this.OnTransitionNotFound;
        }

        /// <summary>
        /// Create a new state and add it to this state machine
        /// </summary>
        /// <param name="name">Name given to the state</param>
        /// <param name="data">Optional data associated with the new state</param>
        /// <returns>The new state</returns>
        public State<TStateData> CreateState(string name, TStateData data = default(TStateData))
        {
            var state = new State<TStateData>(name, data, this);
            this.InnerStateMachine.AddState(state);
            return state;
        }

        /// <summary>
        /// Create the state which this state machine will be in when it first starts. This must be called exactly once per state machine
        /// </summary>
        /// <param name="name">Name given to the state</param>
        /// <param name="data">Optional data associated with the new state</param>
        /// <returns>The new state</returns>
        public State<TStateData> CreateInitialState(string name, TStateData data = default(TStateData))
        {
            var state = this.CreateState(name, data);
            this.InnerStateMachine.SetInitialState(state);
            return state;
        }

        /// <summary>
        /// Create an event which can be used on this state machine, or its children
        /// </summary>
        /// <param name="name">Name given to the event</param>
        /// <returns>The new event</returns>
        public Event CreateEvent(string name)
        {
            return this.InnerStateMachine.CreateEvent(name);
        }

        /// <summary>
        /// Create an event which has associated data which can be used on this state machine, or its children
        /// </summary>
        /// <param name="name">Name given to the event</param>
        /// <returns>The new event</returns>
        public Event<TEventData> CreateEvent<TEventData>(string name)
        {
            return this.InnerStateMachine.CreateEvent<TEventData>(name);
        }

        /// <summary>
        /// Force a transition to the given state, even though there may not be a valid configured transition to that state from the current state
        /// </summary>
        /// <remarks>Exit and entry handlers will be fired, but no transition handler will be fired</remarks>
        /// <param name="toState">State to transition to</param>
        /// <param name="evt">Event pass to the exit/entry handlers</param>
        public void ForceTransition(State<TStateData> toState, IEvent evt)
        {
            if (toState == null)
                throw new ArgumentNullException("toState");
            if (evt == null)
                throw new ArgumentNullException("evt");

            if (toState.ParentStateMachine != this)
                throw new InvalidStateTransitionException(this.CurrentState, toState);
            if (evt.ParentStateMachine != this && !this.IsChildOf(evt.ParentStateMachine))
                throw new InvalidEventTransitionException(this.CurrentState, evt);

            this.InnerStateMachine.ForceTransition(this.CurrentState, toState, toState, evt);
        }

        /// <summary>
        /// Determines whether this state machine is a child of another state machine
        /// </summary>
        /// <param name="parentStateMachine">State machine which may be a parent of this state machine</param>
        /// <returns>True if this state machine is a child of the given state machine</returns>
        public bool IsChildOf(IStateMachine parentStateMachine)
        {
            if (parentStateMachine == null)
                throw new ArgumentNullException("parentStateMachine");

            return this.InnerStateMachine.IsChildOf(parentStateMachine);
        }

        /// <summary>
        /// Determines whether this state machine is in the given state, or the current state's child state machine is in the given state, recursively
        /// </summary>
        /// <param name="state">The state to test</param>
        /// <returns>True if this state machine is in the given state, or the current state's child state machine is in the given state, recursively</returns>
        public bool IsInStateRecursive(IState state)
        {
            if (state == null)
                throw new ArgumentNullException("state");

            return this.InnerStateMachine.IsInStateRecursive(state);
        }

        internal void ResetChildStateMachine()
        {
            this.InnerStateMachine.Reset();
        }

        void IStateDelegate<State<TStateData>>.ForceTransition(State<TStateData> pretendFromState, State<TStateData> pretendToState, State<TStateData> toState, IEvent evt)
        {
            this.InnerStateMachine.ForceTransition(pretendFromState, pretendToState, toState, evt);
        }

        void ITransitionDelegate<State<TStateData>>.UpdateCurrentState(State<TStateData> from, State<TStateData> state, IEvent evt, bool isInnerTransition)
        {
            this.InnerStateMachine.UpdateCurrentState(from, state, evt, isInnerTransition);
        }

        bool IStateMachine<State<TStateData>>.RequestEventFire(IEvent sourceEvent, Func<IState, bool> invoker, bool throwIfNotFound)
        {
            return this.InnerStateMachine.RequestEventFire(sourceEvent, invoker, throwIfNotFound);
        }

        bool IEventDelegate.RequestEventFireFromEvent(IEvent sourceEvent, Func<IState, bool> invoker, bool throwIfNotFound)
        {
            return this.InnerStateMachine.RequestEventFireFromEvent(sourceEvent, invoker, throwIfNotFound);
        }

        private void OnTransition(object sender, TransitionEventArgs<State<TStateData>> eventArgs)
        {
            var handler = this.Transition;
            if (handler != null)
                handler(this, eventArgs);
        }

        private void OnTransitionNotFound(object sender, TransitionNotFoundEventArgs<State<TStateData>> eventArgs)
        {
            var handler = this.TransitionNotFound;
            if (handler != null)
                handler(this, eventArgs);
        }
    }
}
