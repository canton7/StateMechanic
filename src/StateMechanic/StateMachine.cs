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
        public string Name { get; private set; }

        private readonly IStateMachineParent parentStateMachine;
        private readonly Queue<Func<IState, bool>> eventQueue = new Queue<Func<IState, bool>>();

        private bool executingTransition;

        public StateMachineInner(string name, IStateMachineParent parentStateMachine)
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

        IState IEventDelegate.CurrentState
        {
            get { return this.CurrentState; }
        }

        /// <summary>
        /// Attempt to fire an event
        /// </summary>
        /// <param name="invoker">Action which actually triggers the transition. Takes the state to transition from, and returns whether the transition was found</param>
        /// <returns></returns>
        public bool RequestEventFire(Func<IState, bool> invoker)
        {
            if (this.CurrentState == null)
            {
                if (this.InitialState == null)
                    throw new InvalidOperationException("Initial state not yet set. You must call CreateInitialState");
                else
                    throw new InvalidOperationException("Child state machine's parent state is not current. This state machine is currently disabled");
            }

            // TODO Not sure if this should be above the check above...
            if (this.executingTransition)
            {
                this.EnqueueEventFire(invoker);
                return true; // We don't know whether it succeeded or failed, so pretend it succeeded
            }

            bool success;

            // Try and fire it on the child state machine - see if that works
            if (this.CurrentState != null && this.CurrentState.RequestEventFire(invoker))
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

        public void EnqueueEventFire(Func<IState, bool> invoker)
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
                    this.RequestEventFire(this.eventQueue.Dequeue());
                }
            }
        }

        public void UpdateCurrentState(TState state)
        {
            this.CurrentState = state;
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
    }

    public class StateMachine : IStateMachine
    {
        internal StateMachineInner<State> InnerStateMachine { get; private set; }

        public State CurrentState { get { return this.InnerStateMachine.CurrentState; } }
        public State InitialState { get { return this.InnerStateMachine.InitialState; } }
        public string Name { get { return this.InnerStateMachine.Name; } }

        public StateMachine(string name)
            : this(name, null)
        { }

        internal StateMachine(string name, IStateMachineParent parentStateMachine)
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

        internal bool RequestEventFire(Func<IState, bool> invoker)
        {
            return this.InnerStateMachine.RequestEventFire(invoker);
        }
    }

    public class StateMachine<TStateData> : IStateMachine
    {
        internal StateMachineInner<State<TStateData>> InnerStateMachine { get; private set; }

        public State<TStateData> CurrentState { get { return this.InnerStateMachine.CurrentState; } }
        public State<TStateData> InitialState { get { return this.InnerStateMachine.InitialState; } }
        public string Name { get { return this.InnerStateMachine.Name; } }

        public StateMachine(string name)
            : this(name, null)
        { }

        internal StateMachine(string name, IStateMachineParent parentStateMachine)
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

        internal bool RequestEventFire(Func<IState, bool> invoker)
        {
            return this.InnerStateMachine.RequestEventFire(invoker);
        }
    }
}
