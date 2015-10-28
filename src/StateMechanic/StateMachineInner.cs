using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace StateMechanic
{
    internal class StateMachineInner<TState> where TState : class, IState<TState>
    {
        public StateMachineKernel<TState> Kernel { get; private set; }
        private readonly IStateMachine<TState> outerStateMachine;

        private readonly TState parentState;
        private IStateMachine<TState> parentStateMachine
        {
            get { return this.parentState == null ? null : this.parentState.ParentStateMachine; }
        }

        private readonly List<TState> states = new List<TState>();

        public TState InitialState { get; private set; }

        private TState _currentState;
        public TState CurrentState
        {
            get
            {
                if (this.Kernel.Fault != null)
                    throw new StateMachineFaultedException(this.Kernel.Fault);

                return this._currentState;
            }
            set
            {
                this._currentState = value;
            }
        }

        public TState CurrentChildState
        {
            get
            {
                if (this.CurrentState != null && this.CurrentState.ChildStateMachine != null)
                    return this.CurrentState.ChildStateMachine.CurrentChildState;
                else
                    return this.CurrentState;
            }
        }
        public string Name { get; private set; }
        public IStateMachine StateMachine { get { return this.outerStateMachine; } }
        public IReadOnlyList<TState> States { get { return new ReadOnlyCollection<TState>(this.states); } }

        public StateMachineInner(string name, StateMachineKernel<TState> kernel, IStateMachine<TState> outerStateMachine, TState parentState)
        {
            this.Name = name;
            this.Kernel = kernel;
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

        public void ForceTransitionFromUser(ITransitionInvoker<TState> transitionInvoker)
        {
            if (this.Kernel.Synchronizer != null)
                this.Kernel.Synchronizer.ForceTransition(() => this.InvokeTransition(this.ForceTransitionFromUserImpl, transitionInvoker));
            else
                this.InvokeTransition(this.ForceTransitionFromUserImpl, transitionInvoker);
        }

        private bool ForceTransitionFromUserImpl(ITransitionInvoker<TState> transitionInvoker)
        {
            transitionInvoker.TryInvoke(this.CurrentState);
            return true;
            //this.Kernel.CoordinateTransition(this.CurrentState, toState, @event, false, null);
            //return true;
        }

        // invoker: Action which actually triggers the transition. Takes the state to transition from, and returns whether the transition was found
        public bool RequestEventFireFromEvent(ITransitionInvoker<TState> transitionInvoker)
        {
            // TODO: I don't like how many delegates are created here

            if (this.Kernel.Synchronizer != null)
                return this.Kernel.Synchronizer.FireEvent(() => this.InvokeTransition(this.RequestEventFire, transitionInvoker), transitionInvoker.EventFireMethod);
            else
                return this.InvokeTransition(this.RequestEventFire, transitionInvoker);
        }

        private bool RequestEventFire(ITransitionInvoker<TState> transitionInvoker)
        {
            return this.RequestEventFire(transitionInvoker, overrideNoThrow: false);
        }

        public bool RequestEventFire(ITransitionInvoker<TState> transitionInvoker, bool overrideNoThrow)
        {
            this.EnsureCurrentStateSuitableForTransition();

            bool success;

            // Try and fire it on the child state machine - see if that works
            // If we got to here, this.CurrentState != null
            var childStateMachine = this.CurrentState.ChildStateMachine;
            if (childStateMachine != null && childStateMachine.RequestEventFire(transitionInvoker, overrideNoThrow: true))
            {
                success = true;
            }
            else
            {
                // No? Invoke it on ourselves
                success = transitionInvoker.TryInvoke(this.CurrentState);

                if (!success)
                    this.HandleTransitionNotFound(transitionInvoker.Event, throwException: !overrideNoThrow && transitionInvoker.EventFireMethod == EventFireMethod.Fire);
            }

            return success;
        }

        private void EnsureCurrentStateSuitableForTransition()
        {
            if (this.CurrentState == null)
            {
                if (this.InitialState == null)
                    throw new InvalidOperationException("Initial state not yet set. You must call CreateInitialState");
                else
                    throw new InvalidOperationException("Child state machine's parent state is not current. This state machine is currently disabled");
            }
        }


        private bool InvokeTransition(Func<ITransitionInvoker<TState>, bool> method, ITransitionInvoker<TState> transitionInvoker)
        {
            if (this.Kernel.Fault != null)
                throw new StateMachineFaultedException(this.Kernel.Fault);

            if (this.Kernel.ExecutingTransition)
            {
                this.Kernel.EnqueueTransition(method, transitionInvoker);
                return true;
            }

            bool success;

            try
            {
                this.Kernel.ExecutingTransition = true;
                success = method(transitionInvoker);
            }
            catch (InternalTransitionFaultException e)
            {
                var faultInfo = new StateMachineFaultInfo(this.outerStateMachine, e.FaultedComponent, e.InnerException, e.From, e.To, e.Event, e.Group);
                this.Kernel.SetFault(faultInfo);
                throw new TransitionFailedException(faultInfo);
            }
            finally
            {
                this.Kernel.ExecutingTransition = false;
            }


            this.Kernel.FireQueuedTransitions();

            return success;
        }

        public void InitiateReset()
        {
            if (this.Kernel.Synchronizer != null)
                this.Kernel.Synchronizer.Reset(this.ResetInternal);
            else
                this.ResetInternal();
        }

        private void ResetInternal()
        {
            this.Kernel.Reset();
            this.Reset();
        }

        public void Reset()
        {
            // We need to reset our current state before resetting any child state machines, as the
            // child state machine's current state depends on whether or not we're active

            this.ResetCurrentState();

            foreach (var state in this.states)
            {
                state.Reset();
            }
        }

        private void HandleTransitionNotFound(IEvent @event, bool throwException)
        {
            this.Kernel.OnTransitionNotFound(this.CurrentState, @event, this.outerStateMachine);

            if (throwException)
                throw new TransitionNotFoundException(this.CurrentState, @event, this.outerStateMachine);
        }

        public void SetCurrentState(TState state)
        {
            this.CurrentState = state;
        }

        public bool IsChildOf(IStateMachine parentStateMachine)
        {
            if (this.parentStateMachine != null)
                return this.parentStateMachine == parentStateMachine || this.parentStateMachine.IsChildOf(parentStateMachine);

            return false;
        }
    }
}
