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
        public StateMachineKernel<TState> Kernel { get; private set; }
        private readonly IStateMachine<TState> outerStateMachine;

        private readonly TState parentState;
        private IStateMachine<TState> parentStateMachine
        {
            get { return this.parentState == null ? null : this.parentState.ParentStateMachine; }
        }

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
        public IReadOnlyList<TState> States { get { return this.states; } }

        public event EventHandler<TransitionEventArgs<TState>> Transition;
        public event EventHandler<TransitionNotFoundEventArgs<TState>> TransitionNotFound;

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

        public void ForceTransition(TState pretendOldState, TState pretendNewState, TState newState, IEvent evt)
        {
            if (this.Kernel.Synchronizer != null)
                this.Kernel.Synchronizer.ForceTransition(() => this.ForceTransitionImpl(pretendNewState, pretendNewState, newState, evt));
            else
                this.ForceTransitionImpl(pretendOldState, pretendNewState, newState, evt);
        }

        private void ForceTransitionImpl(TState pretendOldState, TState pretendNewState, TState newState, IEvent evt)
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
        public bool RequestEventFireFromEvent(IEvent sourceEvent, Func<IState, bool> invoker, bool throwIfNotFound)
        {
            if (this.Kernel.Synchronizer != null)
                return this.Kernel.Synchronizer.FireEvent(() => this.RequestEventFire(sourceEvent, invoker, throwIfNotFound));
            else
                return this.RequestEventFire(sourceEvent, invoker, throwIfNotFound);
        }

        public bool RequestEventFire(IEvent sourceEvent, Func<IState, bool> invoker, bool throwIfNotFound)
        {
            if (this.Kernel.Fault != null)
                throw new StateMachineFaultedException(this.Kernel.Fault);

            if (this.Kernel.ExecutingTransition)
            {
                // This may end up being fired from a parent state machine. We reference 'this' here so that it's actually executed on us
                this.Kernel.EnqueueEventFire(() => this.RequestEventFire(sourceEvent, invoker, throwIfNotFound));
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
                    this.Kernel.ExecutingTransition = true;
                    success = invoker(this.CurrentState);

                    if (!success)
                        this.HandleTransitionNotFound(sourceEvent, throwIfNotFound);
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

        public void Reset()
        {
            foreach (var state in this.states)
            {
                state.Reset();
            }

            this.ResetCurrentState();
        }

        private void HandleTransitionNotFound(IEvent evt, bool throwException)
        {
            this.OnTransitionNotFound(this.CurrentState, evt);
            this.Kernel.OnTransitionNotFound(this.CurrentState, evt);

            if (throwException)
                throw new TransitionNotFoundException(this.CurrentState, evt);
        }

        public void UpdateCurrentState(TState from, TState to, IEvent evt, bool isInnerTransition)
        {
            this.CurrentState = to;
            this.OnTransition(from, to, evt, isInnerTransition);
            this.Kernel.OnTransition(from, to, evt, isInnerTransition);
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

        private void OnTransitionNotFound(TState from, IEvent evt)
        {
            var handler = this.TransitionNotFound;
            if (handler != null)
                handler(this, new TransitionNotFoundEventArgs<TState>(from, evt));
        }
    }
}
