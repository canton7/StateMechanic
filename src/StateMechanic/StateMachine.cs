using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    internal class StateMachineInner<TState> : ITransitionDelegate<TState>, IEventDelegate where TState : IState<TState>
    {
        public TState CurrentState;
        public string Name { get; private set; }

        // Used to determine whether another event was fired while we were processing a previous event
        private int eventFireCount;

        private int recursionCount;

        public StateMachineInner(string name)
        {
            this.Name = name;
        }

        public void SetInitialState(TState state)
        {
            if (this.CurrentState != null)
                throw new InvalidOperationException("InitialState has arleady been set");

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

        IState IEventDelegate.CurrentState
        {
            get { return this.CurrentState; }
        }

        void IEventDelegate.FireEvent(Action<TransitionInvocationState> invoker)
        {
            this.eventFireCount++;
            this.recursionCount++;

            var state = new TransitionInvocationState(this.eventFireCount);
            invoker(state);
        }

        void ITransitionDelegate<TState>.UpdateCurrentState(TState state)
        {
            this.CurrentState = state;
            this.recursionCount--;
        }

        void ITransitionDelegate<TState>.AddTransition(Event evt, Transition<TState> transition)
        {
            evt.AddTransition(transition.From, transition);
        }

        void ITransitionDelegate<TState>.AddTransition<TEventData>(Event<TEventData> evt, Transition<TState, TEventData> transition)
        {
            evt.AddTransition(transition.From, transition);
        }

        bool ITransitionDelegate<TState>.HasOtherEventBeenFired(TransitionInvocationState transitionInvocationState)
        {
            return this.eventFireCount != transitionInvocationState.EventFireCount;
        }

        bool ITransitionDelegate<TState>.ShouldCallExitHandler(TransitionInvocationState transitionInvocationState)
        {
            return this.recursionCount == 1;
        }
    }

    public class StateMachine
    {
        private readonly StateMachineInner<State> innerStateMachine;

        public State CurrentState { get { return this.innerStateMachine.CurrentState; } }

        public StateMachine(string name)
        {
            this.innerStateMachine = new StateMachineInner<State>(name);
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
    }

    public class StateMachine<TStateData>
    {
        private readonly StateMachineInner<State<TStateData>> innerStateMachine;

        public State<TStateData> CurrentState { get { return this.innerStateMachine.CurrentState; } }

        public StateMachine(string name)
        {
            this.innerStateMachine = new StateMachineInner<State<TStateData>>(name);
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
    }
}
