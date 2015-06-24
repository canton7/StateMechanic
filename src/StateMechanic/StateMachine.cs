using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    internal class StateMachineInner<TState> : IStateDelegate<TState>, IEventDelegate where TState : IState<TState>
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
        IState IStateMachine.CurrentState { get { return this.CurrentState; } }

        public event EventHandler<TransitionEventArgs<TState>> Transition;
        public event EventHandler<TransitionEventArgs<TState>> RecursiveTransition;
        public event EventHandler<TransitionNotFoundEventArgs<TState>> TransitionNotFound;
        public event EventHandler<TransitionNotFoundEventArgs<TState>> RecursiveTransitionNotFound;

        private readonly IStateMachineParent<TState> parentStateMachine;
        private readonly Queue<Func<bool>> eventQueue = new Queue<Func<bool>>();

        private bool executingTransition;

        public StateMachineInner(string name, IStateMachineParent<TState> parentStateMachine)
        {
            this.Name = name;
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
        public bool RequestEventFire(Func<IState, bool> invoker)
        {
            if (this.executingTransition)
            {
                // This may end up being fired from a parent state machine. We reference 'this' here so that it's actually executed on us
                this.EnqueueEventFire(() => this.RequestEventFire(invoker));
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
            if (childStateMachine != null && childStateMachine.RequestEventFire(invoker))
            {
                success = true;
            }
            else
            {
                // No? Invoke it on ourselves
                success = invoker(this.CurrentState);
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
                    // TODO I'm not sure whether such "recursive" events should affect the success of the overall transition...
                    // It may be that we want to remove 'event.TryFire()', and have everything succeed.
                    this.eventQueue.Dequeue()();
                }
            }
        }

        public void UpdateCurrentState(TState from, TState to, IEvent evt)
        {
            this.CurrentState = to;
            this.OnTransition(from, to, evt);
            this.OnRecursiveTransition(from, to, evt);
        }

        public void AddTransition(Event evt, Transition<TState> transition)
        {
            evt.AddTransition(transition.From, transition);
        }

        public void AddTransition<TEventData>(Event<TEventData> evt, Transition<TState, TEventData> transition)
        {
            evt.AddTransition(transition.From, transition);
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

        public void NotifyTransitionNotFound(IEvent evt)
        {
            this.OnTransitionNotFound(this.CurrentState, evt);
            this.OnRecursiveTransitionNotFound(this.CurrentState, evt);
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

    public class StateMachine : IStateMachine<State>
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

        public StateMachine(string name)
            : this(name, null)
        { }

        internal StateMachine(string name, IStateMachineParent<State> parentStateMachine)
        {
            this.InnerStateMachine = new StateMachineInner<State>(name, parentStateMachine);
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

        bool IStateMachine<State>.RequestEventFire(Func<IState, bool> invoker)
        {
            return this.InnerStateMachine.RequestEventFire(invoker);
        }
    }

    public class StateMachine<TStateData> : IStateMachine<State<TStateData>>
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

        public StateMachine(string name)
            : this(name, null)
        { }

        internal StateMachine(string name, IStateMachineParent<State<TStateData>> parentStateMachine)
        {
            this.InnerStateMachine = new StateMachineInner<State<TStateData>>(name, parentStateMachine);
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

        bool IStateMachine<State<TStateData>>.RequestEventFire(Func<IState, bool> invoker)
        {
            return this.InnerStateMachine.RequestEventFire(invoker);
        }
    }
}
