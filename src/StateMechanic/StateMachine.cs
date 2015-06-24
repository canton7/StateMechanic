using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    internal class StateMachineInner<TState> : IStateDelegate<TState>, IEventDelegate where TState : class, IState<TState>
    {
        public TState InitialState { get; private set; }
        public TState CurrentState { get; private set; }
        public TState CurrentStateRecursive
        {
            get
            {
                if (this.CurrentState != null && this.CurrentState.ChildStateMachine != null)
                    return this.CurrentState.ChildStateMachine.CurrentStateRecursive;
                else
                    return this.CurrentState;
            }
        }
        public string Name { get; private set; }
        public IStateMachine StateMachine { get { return this.outerStateMachine; } }

        public event EventHandler<TransitionEventArgs<TState>> Transition;
        public event EventHandler<TransitionEventArgs<TState>> RecursiveTransition;
        public event EventHandler<TransitionNotFoundEventArgs<TState>> TransitionNotFound;
        public event EventHandler<TransitionNotFoundEventArgs<TState>> RecursiveTransitionNotFound;

        private readonly IStateMachine outerStateMachine;
        private readonly IStateMachineParent<TState> parentStateMachine;
        private readonly Queue<Func<bool>> eventQueue = new Queue<Func<bool>>();

        private bool executingTransition;

        public StateMachineInner(string name, IStateMachine outerStateMachine, IStateMachineParent<TState> parentStateMachine)
        {
            this.Name = name;
            this.outerStateMachine = outerStateMachine;
            this.parentStateMachine = parentStateMachine;
        }

        public void SetInitialState(TState state)
        {
            if (this.InitialState != null)
                throw new InvalidOperationException("Initial state has already been set");

            this.InitialState = state;

            // Child state machines start off in no state, and progress to the initial state
            // Normal state machines start in the start state
            if (this.parentStateMachine == null)
                this.CurrentState = state;
        }

        public Event CreateEvent(string name)
        {
            return new Event(name, this);
        }

        public Event<TEventData> CreateEvent<TEventData>(string name)
        {
            return new Event<TEventData>(name, this);
        }

        internal void ForceTransition(TState pretendOldState, TState pretendNewState, TState newState, IEvent evt)
        {
            var handlerInfo = new StateHandlerInfo<TState>(pretendOldState, pretendNewState, evt);

            if (this.CurrentState != null)
                this.CurrentState.FireOnExit(handlerInfo);

            this.CurrentState = newState;

            if (this.CurrentState != null)
                this.CurrentState.FireOnEntry(handlerInfo);
        }

        /// <summary>
        /// Attempt to fire an event
        /// </summary>
        /// <param name="invoker">Action which actually triggers the transition. Takes the state to transition from, and returns whether the transition was found</param>
        /// <returns></returns>
        public bool RequestEventFire(IEvent sourceEvent, Func<IState, bool> invoker, bool throwIfNotFound)
        {
            if (this.executingTransition)
            {
                // This may end up being fired from a parent state machine. We reference 'this' here so that it's actually executed on us
                this.EnqueueEventFire(() => this.RequestEventFire(sourceEvent, invoker, throwIfNotFound));
                return true; // We don't know whether it succeeded or failed, so pretend it succeeded
            }

            if (this.CurrentState == null)
            {
                if (this.InitialState == null)
                    throw new InvalidOperationException("Initial state not yet set. You must call CreateInitialState");
                else
                    throw new InvalidOperationException("Child state machine's parent state is not current. This state machine is currently disabled");
            }

            bool success;

            // Try and fire it on the child state machine - see if that works
            var childStateMachine = this.CurrentState == null ? null : this.CurrentState.ChildStateMachine;
            if (childStateMachine != null && childStateMachine.RequestEventFire(sourceEvent, invoker, false))
            {
                success = true;
            }
            else
            {
                // No? Invoke it on ourselves
                success = invoker(this.CurrentState);
                if (!success)
                    this.HandleTransitionNotFound(sourceEvent, throwIfNotFound);
            }

            this.FireQueuedEvents();
            
            return success;
        }

        public void EnqueueEventFire(Func<bool> invoker)
        {
            if (this.parentStateMachine != null)
                this.parentStateMachine.EnqueueEventFire(invoker);
            else
                this.eventQueue.Enqueue(invoker);
        }

        public void FireQueuedEvents()
        {
            if (this.parentStateMachine != null)
            {
                this.parentStateMachine.FireQueuedEvents();
            }
            else
            {
                while (this.eventQueue.Count > 0)
                {
                    // TODO: Not sure whether these failing should affect the status of the outer parent transition...
                    this.eventQueue.Dequeue()();
                }
            }
        }

        private void HandleTransitionNotFound(IEvent evt, bool throwException)
        {
            this.OnTransitionNotFound(this.CurrentState, evt);
            this.OnRecursiveTransitionNotFound(this.CurrentState, evt);

            if (throwException)
                throw new TransitionNotFoundException(this.CurrentState, evt);
        }

        public void UpdateCurrentState(TState from, TState to, IEvent evt)
        {
            this.CurrentState = to;
            this.OnTransition(from, to, evt);
            this.OnRecursiveTransition(from, to, evt);
        }

        public void TransitionBegan()
        {
            Debug.Assert(!this.executingTransition);
            this.executingTransition = true;
            // Make sure everyone's aware of this fact
            if (this.parentStateMachine != null)
                this.parentStateMachine.TransitionBegan();
        }

        public void TransitionEnded()
        {
            Debug.Assert(this.executingTransition);
            this.executingTransition = false;
            // Make sure everyone's aware of this fact
            if (this.parentStateMachine != null)
                this.parentStateMachine.TransitionEnded();
        }

        public bool IsChildOf(IStateMachine parentStateMachine)
        {
            if (this.parentStateMachine != null)
                return this.parentStateMachine.StateMachine == parentStateMachine || this.parentStateMachine.StateMachine.IsChildOf(parentStateMachine);

            return false;
        }

        public bool IsInState(IState state)
        {
            if (this.CurrentState == null)
                return false;

            return this.CurrentState == state || (this.CurrentState.ChildStateMachine != null && this.CurrentState.ChildStateMachine.IsInState(state));
        }

        private void OnTransition(TState from, TState to, IEvent evt)
        {
            var handler = this.Transition;
            if (handler != null)
                handler(this, new TransitionEventArgs<TState>(from, to, evt));
        }

        public void OnRecursiveTransition(TState from, TState to, IEvent evt)
        {
            if (this.parentStateMachine != null)
                this.parentStateMachine.OnRecursiveTransition(from, to, evt);

            var handler = this.RecursiveTransition;
            if (handler != null)
                handler(this, new TransitionEventArgs<TState>(from, to, evt));
        }

        private void OnTransitionNotFound(TState from, IEvent evt)
        {
            var handler = this.TransitionNotFound;
            if (handler != null)
                handler(this, new TransitionNotFoundEventArgs<TState>(from, evt));
        }

        public void OnRecursiveTransitionNotFound(TState from, IEvent evt)
        {
            if (this.parentStateMachine != null)
                this.parentStateMachine.OnRecursiveTransitionNotFound(from, evt);

            var handler = this.RecursiveTransitionNotFound;
            if (handler != null)
                handler(this, new TransitionNotFoundEventArgs<TState>(from, evt));
        }
    }

    public class StateMachine : IStateMachine<State>, IStateMachine
    {
        internal StateMachineInner<State> InnerStateMachine { get; private set; }

        public State CurrentState { get { return this.InnerStateMachine.CurrentState; } }
        public State CurrentStateRecursive { get { return this.InnerStateMachine.CurrentStateRecursive; } }
        public State InitialState { get { return this.InnerStateMachine.InitialState; } }
        public string Name { get { return this.InnerStateMachine.Name; } }
        IState IStateMachine.CurrentState { get { return this.InnerStateMachine.CurrentState; } }

        public event EventHandler<TransitionEventArgs<State>> Transition
        {
            add { this.InnerStateMachine.Transition += value; }
            remove { this.InnerStateMachine.Transition -= value; }
        }
        public event EventHandler<TransitionEventArgs<State>> RecursiveTransition
        {
            add { this.InnerStateMachine.RecursiveTransition += value; }
            remove { this.InnerStateMachine.RecursiveTransition -= value; }
        }
        public event EventHandler<TransitionNotFoundEventArgs<State>> TransitionNotFound
        {
            add { this.InnerStateMachine.TransitionNotFound += value; }
            remove { this.InnerStateMachine.TransitionNotFound -= value; }
        }
        public event EventHandler<TransitionNotFoundEventArgs<State>> RecursiveTransitionNotFound
        {
            add { this.InnerStateMachine.RecursiveTransitionNotFound += value; }
            remove { this.InnerStateMachine.RecursiveTransitionNotFound -= value; }
        }

        public StateMachine(string name)
            : this(name, null)
        { }

        internal StateMachine(string name, IStateMachineParent<State> parentStateMachine)
        {
            this.InnerStateMachine = new StateMachineInner<State>(name, this, parentStateMachine);
        }

        public State CreateState(string name)
        {
            return new State(name, this.InnerStateMachine);
        }

        public State CreateInitialState(string name)
        {
            var state = this.CreateState(name);
            this.InnerStateMachine.SetInitialState(state);
            return state;
        }

        public Event CreateEvent(string name)
        {
            return this.InnerStateMachine.CreateEvent(name);
        }

        public Event<TEventData> CreateEvent<TEventData>(string name)
        {
            return this.InnerStateMachine.CreateEvent<TEventData>(name);
        }

        public void ForceTransition(State toState, IEvent evt)
        {
            this.InnerStateMachine.ForceTransition(this.CurrentState, toState, toState, evt);
        }

        internal void ForceTransition(State pretendFromState, State pretendToState, State toState, IEvent evt)
        {
            this.InnerStateMachine.ForceTransition(pretendFromState, pretendToState, toState, evt);
        }

        bool IStateMachine<State>.RequestEventFire(IEvent sourceEvent, Func<IState, bool> invoker, bool throwIfNotFound)
        {
            return this.InnerStateMachine.RequestEventFire(sourceEvent, invoker, throwIfNotFound);
        }

        public bool IsChildOf(IStateMachine parentStateMachine)
        {
            return this.InnerStateMachine.IsChildOf(parentStateMachine);
        }

        public bool IsInState(IState state)
        {
            return this.InnerStateMachine.IsInState(state);
        }
    }

    public class StateMachine<TStateData> : IStateMachine<State<TStateData>>, IStateMachine
    {
        internal StateMachineInner<State<TStateData>> InnerStateMachine { get; private set; }

        public State<TStateData> CurrentState { get { return this.InnerStateMachine.CurrentState; } }
        public State<TStateData> CurrentStateRecursive { get { return this.InnerStateMachine.CurrentStateRecursive; } }
        public State<TStateData> InitialState { get { return this.InnerStateMachine.InitialState; } }
        public string Name { get { return this.InnerStateMachine.Name; } }
        IState IStateMachine.CurrentState { get { return this.InnerStateMachine.CurrentState; } }

        public event EventHandler<TransitionEventArgs<State<TStateData>>> Transition
        {
            add { this.InnerStateMachine.Transition += value; }
            remove { this.InnerStateMachine.Transition -= value; }
        }
        public event EventHandler<TransitionEventArgs<State<TStateData>>> RecursiveTransition
        {
            add { this.InnerStateMachine.RecursiveTransition += value; }
            remove { this.InnerStateMachine.RecursiveTransition -= value; }
        }
        public event EventHandler<TransitionNotFoundEventArgs<State<TStateData>>> TransitionNotFound
        {
            add { this.InnerStateMachine.TransitionNotFound += value; }
            remove { this.InnerStateMachine.TransitionNotFound -= value; }
        }
        public event EventHandler<TransitionNotFoundEventArgs<State<TStateData>>> RecursiveTransitionNotFound
        {
            add { this.InnerStateMachine.RecursiveTransitionNotFound += value; }
            remove { this.InnerStateMachine.RecursiveTransitionNotFound -= value; }
        }

        public StateMachine(string name)
            : this(name, null)
        { }

        internal StateMachine(string name, IStateMachineParent<State<TStateData>> parentStateMachine)
        {
            this.InnerStateMachine = new StateMachineInner<State<TStateData>>(name, this, parentStateMachine);
        }

        public State<TStateData> CreateState(string name)
        {
            return new State<TStateData>(name, this.InnerStateMachine);
        }

        public State<TStateData> CreateInitialState(string name)
        {
            var state = this.CreateState(name);
            this.InnerStateMachine.SetInitialState(state);
            return state;
        }

        public Event CreateEvent(string name)
        {
            return this.InnerStateMachine.CreateEvent(name);
        }

        public Event<TEventData> CreateEvent<TEventData>(string name)
        {
            return this.InnerStateMachine.CreateEvent<TEventData>(name);
        }

        public void ForceTransition(State<TStateData> toState, IEvent evt)
        {
            this.InnerStateMachine.ForceTransition(this.CurrentState, toState, toState, evt);
        }

        internal void ForceTransition(State<TStateData> pretendFromState, State<TStateData> pretendToState, State<TStateData> toState, IEvent evt)
        {
            this.InnerStateMachine.ForceTransition(pretendFromState, pretendToState, toState, evt);
        }

        bool IStateMachine<State<TStateData>>.RequestEventFire(IEvent sourceEvent, Func<IState, bool> invoker, bool throwIfNotFound)
        {
            return this.InnerStateMachine.RequestEventFire(sourceEvent, invoker, throwIfNotFound);
        }

        public bool IsChildOf(IStateMachine parentStateMachine)
        {
            return this.InnerStateMachine.IsChildOf(parentStateMachine);
        }

        public bool IsInState(IState state)
        {
            return this.InnerStateMachine.IsInState(state);
        }
    }
}
