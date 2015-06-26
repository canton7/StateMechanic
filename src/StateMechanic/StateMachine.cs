using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    internal class StateMachineInner<TState> where TState : class, IState<TState>
    {
        private readonly IStateMachine<TState> outerStateMachine;

        private readonly TState parentState;
        private IStateMachine<TState> parentStateMachine
        {
            get { return this.parentState == null ? null : this.parentState.ParentStateMachine; }
        }

        private readonly Queue<Func<bool>> eventQueue = new Queue<Func<bool>>();
        private readonly List<TState> states = new List<TState>();

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
        public IReadOnlyList<TState> States { get { return this.states.AsReadOnly(); } }

        public event EventHandler<TransitionEventArgs<TState>> Transition;
        public event EventHandler<TransitionEventArgs<TState>> RecursiveTransition;
        public event EventHandler<TransitionNotFoundEventArgs<TState>> TransitionNotFound;
        public event EventHandler<TransitionNotFoundEventArgs<TState>> RecursiveTransitionNotFound;
        public event EventHandler<StateMachineFaultedEventArgs> Faulted;

        private bool _executingTransition; // Only used if we don't have a parent
        public bool ExecutingTransition
        {
            get
            {
                if (this.parentStateMachine != null)
                    return this.parentStateMachine.ExecutingTransition;
                else
                    return this._executingTransition;
            }
            set
            {
                if (this.parentStateMachine != null)
                    this.parentStateMachine.ExecutingTransition = value;
                else
                    this._executingTransition = value;
            }
        }

        private StateMachineFaultInfo _fault;
        public StateMachineFaultInfo Fault
        {
            get
            {
                if (this.parentStateMachine != null)
                    return this.parentStateMachine.Fault;
                else
                    return this._fault;
            }
            set
            {
                if (this.parentStateMachine != null)
                    this.parentStateMachine.Fault = value;
                else
                    this._fault = value;
            }
        }


        public StateMachineInner(string name, IStateMachine<TState> outerStateMachine, TState parentState)
        {
            this.Name = name;
            this.outerStateMachine = outerStateMachine;
            this.parentState = parentState;
        }

        public void SetInitialState(TState state)
        {
            if (this.InitialState != null)
                throw new InvalidOperationException("Initial state has already been set");

            this.InitialState = state;

            // Child state machines start off in no state, and progress to the initial state
            // Normal state machines start in the start state
            // The exception is child state machines which are children of their parent's initial state, where the parent is not a child state machine

            this.ResetCurrentState();
        }

        private void ResetCurrentState()
        {
            if (this.parentState == null || this.parentState == this.parentState.ParentStateMachine.CurrentState)
                this.CurrentState = this.InitialState;
            else
                this.CurrentState = null;
        }

        public void AddState(TState state)
        {
            this.states.Add(state);
        }

        public Event CreateEvent(string name)
        {
            return new Event(name, this.outerStateMachine);
        }

        public Event<TEventData> CreateEvent<TEventData>(string name)
        {
            return new Event<TEventData>(name, this.outerStateMachine);
        }

        internal void ForceTransition(TState pretendOldState, TState pretendNewState, TState newState, IEvent evt)
        {
            var handlerInfo = new StateHandlerInfo<TState>(pretendOldState, pretendNewState, evt);

            if (this.CurrentState != null)
                this.CurrentState.FireExitHandler(handlerInfo);

            this.CurrentState = newState;

            if (this.CurrentState != null)
                this.CurrentState.FireEntryHandler(handlerInfo);
        }

        /// <summary>
        /// Attempt to fire an event
        /// </summary>
        /// <param name="invoker">Action which actually triggers the transition. Takes the state to transition from, and returns whether the transition was found</param>
        /// <returns></returns>
        public bool RequestEventFire(IEvent sourceEvent, Func<IState, bool> invoker, bool throwIfNotFound)
        {
            if (this.Fault != null)
                throw new StateMachineFaultedException(this.Fault);

            if (this.ExecutingTransition)
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
                try
                {
                    this.TransitionBegan();
                    success = invoker(this.CurrentState);

                    if (!success)
                        this.HandleTransitionNotFound(sourceEvent, throwIfNotFound);
                }
                catch (InternalTransitionFaultException e)
                {
                    var faultInfo = new StateMachineFaultInfo(this.outerStateMachine, e.FaultedComponent, e.InnerException, e.From, e.To, e.Event);
                    this.Fault = faultInfo;
                    this.OnFaulted(faultInfo);
                    throw new TransitionFailedException(faultInfo);
                }
                finally
                {
                    this.TransitionEnded();
                }
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

        public void Reset()
        {
            foreach (var state in this.states)
            {
                state.Reset();
            }

            this.Fault = null;
            this.ResetCurrentState();
        }

        private void HandleTransitionNotFound(IEvent evt, bool throwException)
        {
            this.OnTransitionNotFound(this.CurrentState, evt);
            this.OnRecursiveTransitionNotFound(this.CurrentState, evt);

            if (throwException)
                throw new TransitionNotFoundException(this.CurrentState, evt);
        }

        public void UpdateCurrentState(TState from, TState to, IEvent evt, bool isInnerTransition)
        {
            this.CurrentState = to;
            this.OnTransition(from, to, evt, isInnerTransition);
            this.OnRecursiveTransition(from, to, evt, isInnerTransition);
        }

        public void TransitionBegan()
        {
            Debug.Assert(!this.ExecutingTransition);
            this.ExecutingTransition = true;
        }

        public void TransitionEnded()
        {
            Debug.Assert(this.ExecutingTransition);
            this.ExecutingTransition = false;
        }

        public bool IsChildOf(IStateMachine parentStateMachine)
        {
            if (this.parentStateMachine != null)
                return this.parentStateMachine == parentStateMachine || this.parentStateMachine.IsChildOf(parentStateMachine);

            return false;
        }

        public bool IsInState(IState state)
        {
            if (this.CurrentState == null)
                return false;

            return this.CurrentState == state || (this.CurrentState.ChildStateMachine != null && this.CurrentState.ChildStateMachine.IsInState(state));
        }

        private void OnTransition(TState from, TState to, IEvent evt, bool isInnerTransition)
        {
            var handler = this.Transition;
            if (handler != null)
                handler(this, new TransitionEventArgs<TState>(from, to, evt, isInnerTransition));
        }

        public void OnRecursiveTransition(TState from, TState to, IEvent evt, bool isInnerTransition)
        {
            if (this.parentStateMachine != null)
                this.parentStateMachine.OnRecursiveTransition(from, to, evt, isInnerTransition);

            var handler = this.RecursiveTransition;
            if (handler != null)
                handler(this, new TransitionEventArgs<TState>(from, to, evt, isInnerTransition));
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

        private void OnFaulted(StateMachineFaultInfo faultInfo)
        {
            var handler = this.Faulted;
            if (handler != null)
                handler(this, new StateMachineFaultedEventArgs(faultInfo));
        }
    }

    public class StateMachine : IStateMachine<State>, ITransitionDelegate<State>, IEventDelegate
    {
        internal StateMachineInner<State> InnerStateMachine { get; private set; }

        public State CurrentState { get { return this.InnerStateMachine.CurrentState; } }
        public State CurrentStateRecursive { get { return this.InnerStateMachine.CurrentStateRecursive; } }
        public State InitialState { get { return this.InnerStateMachine.InitialState; } }
        public string Name { get { return this.InnerStateMachine.Name; } }
        IState IStateMachine.CurrentState { get { return this.InnerStateMachine.CurrentState; } }
        IState IStateMachine.CurrentStateRecursive { get { return this.InnerStateMachine.CurrentStateRecursive; } }
        IState IStateMachine.InitialState { get { return this.InnerStateMachine.InitialState; } }
        public IReadOnlyList<State> States { get { return this.InnerStateMachine.States; } }
        IReadOnlyList<IState> IStateMachine.States { get { return this.InnerStateMachine.States; } }
        public StateMachineFaultInfo Fault { get { return this.InnerStateMachine.Fault; } }
        public bool IsFaulted { get { return this.Fault != null; } }

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
        public event EventHandler<StateMachineFaultedEventArgs> Faulted
        {
            add { this.InnerStateMachine.Faulted += value; }
            remove { this.InnerStateMachine.Faulted -= value; }
        }

        public StateMachine(string name)
            : this(name, null)
        { }

        internal StateMachine(string name, State parentState)
        {
            this.InnerStateMachine = new StateMachineInner<State>(name, this, parentState);
        }

        public State CreateState(string name)
        {
            var state = new State(name, this);
            this.InnerStateMachine.AddState(state);
            return state;
        }

        public State CreateInitialState(string name)
        {
            var state = this.CreateState(name);
            this.InnerStateMachine.SetInitialState(state);
            this.InnerStateMachine.AddState(state);
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

        public void Reset()
        {
            this.InnerStateMachine.Reset();
        }

        internal void ForceTransition(State pretendFromState, State pretendToState, State toState, IEvent evt)
        {
            this.InnerStateMachine.ForceTransition(pretendFromState, pretendToState, toState, evt);
        }

        public bool IsChildOf(IStateMachine parentStateMachine)
        {
            return this.InnerStateMachine.IsChildOf(parentStateMachine);
        }

        public bool IsInState(IState state)
        {
            return this.InnerStateMachine.IsInState(state);
        }

        bool IStateMachine<State>.ExecutingTransition
        {
            get { return this.InnerStateMachine.ExecutingTransition; }
            set { this.InnerStateMachine.ExecutingTransition = true; }
        }

        StateMachineFaultInfo IStateMachine<State>.Fault
        {
            get { return this.InnerStateMachine.Fault; }
            set { this.InnerStateMachine.Fault = value; }
        }

        void IStateMachine<State>.EnqueueEventFire(Func<bool> invoker)
        {
            this.InnerStateMachine.EnqueueEventFire(invoker);
        }

        void IStateMachine<State>.FireQueuedEvents()
        {
            this.InnerStateMachine.FireQueuedEvents();
        }

        void IStateMachine<State>.OnRecursiveTransition(State from, State to, IEvent evt, bool isInnerTransition)
        {
            this.InnerStateMachine.OnRecursiveTransition(from, to, evt, isInnerTransition);
        }

        void IStateMachine<State>.OnRecursiveTransitionNotFound(State from, IEvent evt)
        {
            this.InnerStateMachine.OnRecursiveTransitionNotFound(from, evt);
        }

        void ITransitionDelegate<State>.UpdateCurrentState(State from, State state, IEvent evt, bool isInnerSelfTransition)
        {
            this.InnerStateMachine.UpdateCurrentState(from, state, evt, isInnerSelfTransition);
        }

        bool IEventDelegate.RequestEventFire(IEvent sourceEvent, Func<IState, bool> invoker, bool throwIfNotFound)
        {
            return this.InnerStateMachine.RequestEventFire(sourceEvent, invoker, throwIfNotFound);
        }
    }

    public class StateMachine<TStateData> : IStateMachine<State<TStateData>>, ITransitionDelegate<State<TStateData>>, IEventDelegate
    {
        internal StateMachineInner<State<TStateData>> InnerStateMachine { get; private set; }

        public State<TStateData> CurrentState { get { return this.InnerStateMachine.CurrentState; } }
        public State<TStateData> CurrentStateRecursive { get { return this.InnerStateMachine.CurrentStateRecursive; } }
        public State<TStateData> InitialState { get { return this.InnerStateMachine.InitialState; } }
        public string Name { get { return this.InnerStateMachine.Name; } }
        IState IStateMachine.CurrentState { get { return this.InnerStateMachine.CurrentState; } }
        IState IStateMachine.CurrentStateRecursive { get { return this.InnerStateMachine.CurrentStateRecursive; } }
        IState IStateMachine.InitialState { get { return this.InnerStateMachine.InitialState; } }
        public IReadOnlyList<State<TStateData>> States { get { return this.InnerStateMachine.States; } }
        IReadOnlyList<IState> IStateMachine.States { get { return this.InnerStateMachine.States; } }
        public StateMachineFaultInfo Fault { get { return this.InnerStateMachine.Fault; } }
        public bool IsFaulted { get { return this.Fault != null; } }

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
        public event EventHandler<StateMachineFaultedEventArgs> Faulted
        {
            add { this.InnerStateMachine.Faulted += value; }
            remove { this.InnerStateMachine.Faulted -= value; }
        }

        public StateMachine(string name)
            : this(name, null)
        { }

        internal StateMachine(string name, State<TStateData> parentState)
        {
            this.InnerStateMachine = new StateMachineInner<State<TStateData>>(name, this, parentState);
        }

        public State<TStateData> CreateState(string name)
        {
            return new State<TStateData>(name, this);
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

        public void Reset()
        {
            this.InnerStateMachine.Reset();
        }

        internal void ForceTransition(State<TStateData> pretendFromState, State<TStateData> pretendToState, State<TStateData> toState, IEvent evt)
        {
            this.InnerStateMachine.ForceTransition(pretendFromState, pretendToState, toState, evt);
        }

        public bool IsChildOf(IStateMachine parentStateMachine)
        {
            return this.InnerStateMachine.IsChildOf(parentStateMachine);
        }

        public bool IsInState(IState state)
        {
            return this.InnerStateMachine.IsInState(state);
        }

        bool IStateMachine<State<TStateData>>.ExecutingTransition
        {
            get { return this.InnerStateMachine.ExecutingTransition; }
            set { this.InnerStateMachine.ExecutingTransition = true; }
        }

        StateMachineFaultInfo IStateMachine<State<TStateData>>.Fault
        {
            get { return this.InnerStateMachine.Fault; }
            set { this.InnerStateMachine.Fault = value; }
        }

        void IStateMachine<State<TStateData>>.EnqueueEventFire(Func<bool> invoker)
        {
            this.InnerStateMachine.EnqueueEventFire(invoker);
        }

        void IStateMachine<State<TStateData>>.FireQueuedEvents()
        {
            this.InnerStateMachine.FireQueuedEvents();
        }

        void IStateMachine<State<TStateData>>.OnRecursiveTransition(State<TStateData> from, State<TStateData> to, IEvent evt, bool isInnerTransition)
        {
            this.InnerStateMachine.OnRecursiveTransition(from, to, evt, isInnerTransition);
        }

        void IStateMachine<State<TStateData>>.OnRecursiveTransitionNotFound(State<TStateData> from, IEvent evt)
        {
            this.InnerStateMachine.OnRecursiveTransitionNotFound(from, evt);
        }

        void ITransitionDelegate<State<TStateData>>.UpdateCurrentState(State<TStateData> from, State<TStateData> state, IEvent evt, bool isInnerSelfTransition)
        {
            this.InnerStateMachine.UpdateCurrentState(from, state, evt, isInnerSelfTransition);
        }

        bool IEventDelegate.RequestEventFire(IEvent sourceEvent, Func<IState, bool> invoker, bool throwIfNotFound)
        {
            return this.InnerStateMachine.RequestEventFire(sourceEvent, invoker, throwIfNotFound);
        }
    }
}
