using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    internal class StateMachineInner<TState> : ITransitionDelegate<TState>, IEventDelegate where TState : IState<TState>
    {
        public TState InitialState { get; private set; }
        public TState CurrentState { get; private set; }
        public string Name { get; private set; }

        private readonly bool isChildStateMachine;

        private bool executingTransition;

        public StateMachineInner(string name, bool isChildStateMachine)
        {
            this.Name = name;
            this.isChildStateMachine = isChildStateMachine;
        }

        public void SetInitialState(TState state)
        {
            if (this.InitialState != null)
                throw new InvalidOperationException("Initial state has already been set");

            this.InitialState = state;

            // Child state machines start off in no state, and progress to the initial state
            // Normal state machines start in the start state
            if (!this.isChildStateMachine)
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

            // Try and fire it on the child state machine - see if that works
            if (this.CurrentState != null && this.CurrentState.RequestEventFire(invoker))
                return true;

            // No? Invoke it on ourselves
            return invoker(this.CurrentState);
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

        public void SetTransitionBegin()
        {
            Debug.Assert(!this.executingTransition);
            this.executingTransition = true;
        }

        public void SetTransitionEnd()
        {
            Debug.Assert(this.executingTransition);
            this.executingTransition = false;
        }
    }

    public class StateMachine
    {
        private readonly StateMachineInner<State> innerStateMachine;

        public State CurrentState { get { return this.innerStateMachine.CurrentState; } }
        public State InitialState { get { return this.innerStateMachine.InitialState; } }

        public StateMachine(string name)
            : this(name, false)
        { }

        internal StateMachine(string name, bool isChildStateMachine)
        {
            this.innerStateMachine = new StateMachineInner<State>(name, isChildStateMachine);
        }

        public State CreateState(string name)
        {
            return new State(name, this.innerStateMachine);
        }

        public State CreateInitialState(string name)
        {
            var state = this.CreateState(name);
            this.innerStateMachine.SetInitialState(state);
            return state;
        }

        public Event CreateEvent(string name)
        {
            return this.innerStateMachine.CreateEvent(name);
        }

        public Event<TEventData> CreateEvent<TEventData>(string name)
        {
            return this.innerStateMachine.CreateEvent<TEventData>(name);
        }

        public void ForceTransition(State toState, IEvent evt)
        {
            this.innerStateMachine.ForceTransition(this.CurrentState, toState, toState, evt);
        }

        internal void ForceTransition(State pretendFromState, State pretendToState, State toState, IEvent evt)
        {
            this.innerStateMachine.ForceTransition(pretendFromState, pretendToState, toState, evt);
        }

        internal bool RequestEventFire(Func<IState, bool> invoker)
        {
            return this.innerStateMachine.RequestEventFire(invoker);
        }
    }

    public class StateMachine<TStateData>
    {
        private readonly StateMachineInner<State<TStateData>> innerStateMachine;

        public State<TStateData> CurrentState { get { return this.innerStateMachine.CurrentState; } }
        public State<TStateData> InitialState { get { return this.innerStateMachine.InitialState; } }

        public StateMachine(string name)
            : this(name, false)
        { }

        internal StateMachine(string name, bool isChildStateMachine)
        {
            this.innerStateMachine = new StateMachineInner<State<TStateData>>(name, isChildStateMachine);
        }

        public State<TStateData> CreateState(string name)
        {
            return new State<TStateData>(name, this.innerStateMachine);
        }

        public State<TStateData> CreateInitialState(string name)
        {
            var state = this.CreateState(name);
            this.innerStateMachine.SetInitialState(state);
            return state;
        }

        public Event CreateEvent(string name)
        {
            return this.innerStateMachine.CreateEvent(name);
        }

        public Event<TEventData> CreateEvent<TEventData>(string name)
        {
            return this.innerStateMachine.CreateEvent<TEventData>(name);
        }

        public void ForceTransition(State<TStateData> toState, IEvent evt)
        {
            this.innerStateMachine.ForceTransition(this.CurrentState, toState, toState, evt);
        }

        internal void ForceTransition(State<TStateData> pretendFromState, State<TStateData> pretendToState, State<TStateData> toState, IEvent evt)
        {
            this.innerStateMachine.ForceTransition(pretendFromState, pretendToState, toState, evt);
        }

        internal bool RequestEventFire(Func<IState, bool> invoker)
        {
            return this.innerStateMachine.RequestEventFire(invoker);
        }
    }
}
