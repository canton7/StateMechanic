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

        public IEnumerable<TState> GetCurrentChildStates()
        {
            if (this.CurrentState == null)
                yield break;

            yield return this.CurrentState;

            var stateMachine = this.CurrentState.ChildStateMachine;
            while (stateMachine != null)
            {
                if (stateMachine.CurrentState != null)
                {
                    yield return stateMachine.CurrentState;
                    stateMachine = stateMachine.CurrentState.ChildStateMachine;
                }
                else
                {
                    stateMachine = null; // Break the loop
                }
            }
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

        public void ForceTransition(TState pretendOldState, TState pretendNewState, TState newState, IEvent @event)
        {
            if (this.Kernel.Synchronizer != null)
                this.Kernel.Synchronizer.ForceTransition(() => this.ForceTransitionImpl(pretendNewState, pretendNewState, newState, @event));
            else
                this.ForceTransitionImpl(pretendOldState, pretendNewState, newState, @event);
        }

        private void ForceTransitionImpl(TState pretendOldState, TState pretendNewState, TState newState, IEvent @event)
        {
            var handlerInfo = new StateHandlerInfo<TState>(pretendOldState, pretendNewState, @event);

            if (this.CurrentState != null)
                this.CurrentState.FireExitHandler(handlerInfo);

            this.CurrentState = newState;

            if (this.CurrentState != null)
                this.CurrentState.FireEntryHandler(handlerInfo);
        }

        // invoker: Action which actually triggers the transition. Takes the state to transition from, and returns whether the transition was found</param>
        public bool RequestEventFireFromEvent(IEvent sourceEvent, Func<IState, bool> invoker, EventFireMethod fireMethod)
        {
            if (this.Kernel.Synchronizer != null)
                return this.Kernel.Synchronizer.FireEvent(() => this.RequestEventFire(sourceEvent, invoker, fireMethod), fireMethod);
            else
                return this.RequestEventFire(sourceEvent, invoker, fireMethod);
        }

        public bool RequestEventFire(IEvent sourceEvent, Func<IState, bool> invoker, EventFireMethod fireMethod)
        {
            if (this.Kernel.Fault != null)
                throw new StateMachineFaultedException(this.Kernel.Fault);

            if (this.Kernel.ExecutingTransition)
            {
                // This may end up being fired from a parent state machine. We reference 'this' here so that it's actually executed on us
                this.Kernel.EnqueueEventFire(() => this.RequestEventFire(sourceEvent, invoker, fireMethod));
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
            // We do this instead of using of using GetCurrentChildStates as it invokes the CurrentState / InitialState checks
            // If we got to here, this.CurrentState != null
            var childStateMachine = this.CurrentState.ChildStateMachine;
            if (childStateMachine != null && childStateMachine.RequestEventFire(sourceEvent, invoker, EventFireMethod.TryFire))
            {
                success = true;
            }
            else
            {
                // No? Invoke it on ourselves
                try
                {
                    this.Kernel.ExecutingTransition = true;
                    success = invoker(this.CurrentState);

                    if (!success)
                        this.HandleTransitionNotFound(sourceEvent, throwException: fireMethod == EventFireMethod.Fire);
                }
                catch (InternalTransitionFaultException e)
                {
                    var faultInfo = new StateMachineFaultInfo(this.outerStateMachine, e.FaultedComponent, e.InnerException, e.From, e.To, e.Event);
                    this.Kernel.SetFault(faultInfo);
                    throw new TransitionFailedException(faultInfo);
                }
                finally
                {
                    this.Kernel.ExecutingTransition = false;
                }
            }

            this.Kernel.FireQueuedEvents();

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

        public void UpdateCurrentState(TState from, TState to, IEvent @event, bool isInnerTransition)
        {
            this.CurrentState = to;
            this.Kernel.OnTransition(from, to, @event, this.outerStateMachine, isInnerTransition);
        }

        public bool IsChildOf(IStateMachine parentStateMachine)
        {
            if (this.parentStateMachine != null)
                return this.parentStateMachine == parentStateMachine || this.parentStateMachine.IsChildOf(parentStateMachine);

            return false;
        }

        public bool IsInStateRecursive(IState state)
        {
            if (this.CurrentState == null)
                return false;

            return this.CurrentState == state || (this.CurrentState.ChildStateMachine != null && this.CurrentState.ChildStateMachine.IsInStateRecursive(state));
        }
    }
}
