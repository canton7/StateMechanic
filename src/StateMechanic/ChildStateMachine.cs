using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public class ChildStateMachine : IStateMachine<State>, ITransitionDelegate<State>, IEventDelegate
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

        public event EventHandler<TransitionEventArgs<State>> Transition
        {
            add { this.InnerStateMachine.Transition += value; }
            remove { this.InnerStateMachine.Transition -= value; }
        }
        public event EventHandler<TransitionNotFoundEventArgs<State>> TransitionNotFound
        {
            add { this.InnerStateMachine.TransitionNotFound += value; }
            remove { this.InnerStateMachine.TransitionNotFound -= value; }
        }

        internal ChildStateMachine(string name, StateMachineKernel<State> kernel, State parentState)
        {
            this.InnerStateMachine = new StateMachineInner<State>(name, kernel, this, parentState);
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

        internal void ResetSubStateMachine()
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

        void ITransitionDelegate<State>.UpdateCurrentState(State from, State state, IEvent evt, bool isInnerSelfTransition)
        {
            this.InnerStateMachine.UpdateCurrentState(from, state, evt, isInnerSelfTransition);
        }

        bool IStateMachine<State>.RequestEventFire(IEvent sourceEvent, Func<IState, bool> invoker, bool throwIfNotFound)
        {
            return this.InnerStateMachine.RequestEventFire(sourceEvent, invoker, throwIfNotFound);
        }

        bool IEventDelegate.RequestEventFireFromEvent(IEvent sourceEvent, Func<IState, bool> invoker, bool throwIfNotFound)
        {
            return this.InnerStateMachine.RequestEventFireFromEvent(sourceEvent, invoker, throwIfNotFound);
        }
    }

    public class ChildStateMachine<TStateData> : IStateMachine<State<TStateData>>, ITransitionDelegate<State<TStateData>>, IEventDelegate
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

        public event EventHandler<TransitionEventArgs<State<TStateData>>> Transition
        {
            add { this.InnerStateMachine.Transition += value; }
            remove { this.InnerStateMachine.Transition -= value; }
        }
        public event EventHandler<TransitionNotFoundEventArgs<State<TStateData>>> TransitionNotFound
        {
            add { this.InnerStateMachine.TransitionNotFound += value; }
            remove { this.InnerStateMachine.TransitionNotFound -= value; }
        }

        internal ChildStateMachine(string name, StateMachineKernel<State<TStateData>> kernel, State<TStateData> parentState)
        {
            this.InnerStateMachine = new StateMachineInner<State<TStateData>>(name, kernel, this, parentState);
        }

        public State<TStateData> CreateState(string name, TStateData data)
        {
            var state = new State<TStateData>(name, data, this);
            this.InnerStateMachine.AddState(state);
            return state;
        }

        public State<TStateData> CreateInitialState(string name, TStateData data)
        {
            var state = this.CreateState(name, data);
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

        public void ForceTransition(State<TStateData> toState, IEvent evt)
        {
            this.InnerStateMachine.ForceTransition(this.CurrentState, toState, toState, evt);
        }

        internal void ResetSubStateMachine()
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

        void ITransitionDelegate<State<TStateData>>.UpdateCurrentState(State<TStateData> from, State<TStateData> state, IEvent evt, bool isInnerSelfTransition)
        {
            this.InnerStateMachine.UpdateCurrentState(from, state, evt, isInnerSelfTransition);
        }

        bool IStateMachine<State<TStateData>>.RequestEventFire(IEvent sourceEvent, Func<IState, bool> invoker, bool throwIfNotFound)
        {
            return this.InnerStateMachine.RequestEventFire(sourceEvent, invoker, throwIfNotFound);
        }

        bool IEventDelegate.RequestEventFireFromEvent(IEvent sourceEvent, Func<IState, bool> invoker, bool throwIfNotFound)
        {
            return this.InnerStateMachine.RequestEventFireFromEvent(sourceEvent, invoker, throwIfNotFound);
        }
    }
}
