using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    internal class StateMachineInner<TState> : ITransitionRepository<TState> where TState : IState, IState<TState>
    {
        public TState CurrentState;
        public string Name { get; private set; }

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
            return new Event(name, this.FireEvent);
        }

        public Event<TEventData> CreateEvent<TEventData>(string name)
        {
            return new Event<TEventData>(name, this.FireEvent);
        }

        private void FireEvent(Func<IState, TransitionInvoker> transitionInvocation)
        {
            //this.CurrentState.TryFireEvent(evt);
            var invoker = transitionInvocation(this.CurrentState);

            if (invoker != null && invoker.CanInvoke())
            {
                invoker.Invoke();
            }
        }

        void ITransitionRepository<TState>.AddTransition(Event evt, Transition<TState> transition)
        {
            evt.AddTransition(transition.From, transition);
        }

        void ITransitionRepository<TState>.AddTransition<TEventData>(Event<TEventData> evt, Transition<TState, TEventData> transition)
        {
            evt.AddTransition(transition.From, transition);
        }
    }

    public class StateMachine
    {
        private readonly StateMachineInner<State> innerStateMachine;

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
