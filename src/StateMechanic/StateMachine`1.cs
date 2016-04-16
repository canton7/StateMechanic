using System;
using System.Collections.Generic;
using System.Linq;

namespace StateMechanic
{
    /// <summary>
    /// A state machine
    /// </summary>
    public class StateMachine<TState> : ChildStateMachine<TState>, IEventDelegate
        where TState : StateBase<TState>, new()
    {
        internal override StateMachine<TState> TopmostStateMachineInternal => this;

        /// <summary>
        /// Gets the fault associated with this state machine. A state machine will fault if one of its handlers throws an exception
        /// </summary>
        public StateMachineFaultInfo Fault => this.Kernel.Fault;

        /// <summary>
        /// Gets a value indicating whether this state machine is faulted. A state machine will fault if one of its handlers throws an exception
        /// </summary>
        public bool IsFaulted => this.Fault != null;

        /// <summary>
        /// Gets or sets the synchronizer used by this state machine to achieve thread safety. State machines are not thread safe by default
        /// </summary>
        public IStateMachineSynchronizer Synchronizer
        {
            get { return this.Kernel.Synchronizer; }
            set { this.Kernel.Synchronizer = value; }
        }

        private IStateMachineSerializer<TState> serializer = new StateMachineSerializer<TState>();

        /// <summary>
        /// Gets or sets the serializer used by this state machine to serialize or deserialize itself, (see <see cref="Serialize()"/> and <see cref="Deserialize(string)"/>).
        /// </summary>
        public IStateMachineSerializer<TState> Serializer
        {
            get { return this.serializer; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                this.serializer = value;
            }
        }

        /// <summary>
        /// Event raised when a fault occurs in this state machine. A state machine will fault if one of its handlers throws an exception
        /// </summary>
        public event EventHandler<StateMachineFaultedEventArgs> Faulted;

        /// <summary>
        /// Event raised when a transition occurs in this state machine, or any of its child state machines
        /// </summary>
        public event EventHandler<TransitionEventArgs<TState>> Transition;

        /// <summary>
        /// Event raised whenever an event is fired but no corresponding transition is found on this state machine or any of its child state machines
        /// </summary>
        public event EventHandler<TransitionNotFoundEventArgs<TState>> TransitionNotFound;

        /// <summary>
        /// Instantiates a new instance of the <see cref="StateMachine{TState}"/> class, with the given name
        /// </summary>
        /// <param name="name">Name of this state machine</param>
        public StateMachine(string name = null)
            : base(name, new StateMachineKernel<TState>(), null)
        {
            this.Kernel.Faulted += this.OnFaulted;
            this.Kernel.Transition += this.OnTransition;
            this.Kernel.TransitionNotFound += this.OnTransitionNotFound;
        }

        /// <summary>
        /// Force a transition to the given state, even though there may not be a valid configured transition to that state from the current state
        /// </summary>
        /// <remarks>Exit and entry handlers will be fired, but no transition handler will be fired</remarks>
        /// <param name="toState">State to transition to</param>
        /// <param name="event">Event pass to the exit/entry handlers</param>
        public void ForceTransition(TState toState, IEvent @event)
        {
            if (toState == null)
                throw new ArgumentNullException(nameof(toState));
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            var transitionInvoker = new ForcedTransitionInvoker<TState>(toState, @event, this.Kernel);
            if (this.Kernel.Synchronizer != null)
                this.Kernel.Synchronizer.ForceTransition(() => this.InvokeTransition(this.ForceTransitionImpl, transitionInvoker));
            else
                this.InvokeTransition(this.ForceTransitionImpl, transitionInvoker);
        }

        private bool ForceTransitionImpl(ITransitionInvoker<TState> transitionInvoker)
        {
            transitionInvoker.TryInvoke(this.CurrentState);
            return true;
        }

        /// <summary>
        /// Serialize the current state of this state machine into a string, which can later be used to restore the state of the state machine
        /// </summary>
        /// <returns></returns>
        public string Serialize()
        {
            return this.serializer.Serialize(this);
        }

        /// <summary>
        /// Restore the state of this state machine from a previously-created string (from calling the <see cref="Serialize()"/> method).
        /// </summary>
        /// <param name="serialized">String created by <see cref="Serialize()"/></param>
        public void Deserialize(string serialized)
        {
            this.Reset();

            var childState = this.serializer.Deserialize(this, serialized);
            var intermediateStates = new List<TState>();
            for (TState state = childState; state != null; state = state.ParentStateMachine.ParentState)
            {
                intermediateStates.Add(state);
            }

            ChildStateMachine<TState> stateMachine = this;

            foreach (var state in intermediateStates.AsEnumerable().Reverse())
            {
                // We should only hit this if the deserializer is faulty
                if (stateMachine == null)
                    throw new StateMachineSerializationException($"Unable to deserialize from \"{serialized}\": the previous state has no child state machine. Make sure you're deserializing into exactly the same state machine as created the serialized string.");

                // This will throw if the state doesn't belong to the state machine
                stateMachine.SetCurrentState(state);

                stateMachine = state.ChildStateMachine;
            }

            // Did we run out of identifiers?
            // We need to check this to avoid internal inconsistency
            if (stateMachine != null)
                throw new StateMachineSerializationException($"Unable to deserialize from \"{serialized}\": a parent state has the child state machine {stateMachine}, but no information is present in the serialized string saying what its state should be. Make sure you're deserializing into exactly the same state machine as created the serialized string.");
        }

        bool IEventDelegate.RequestEventFireFromEvent(Event @event, EventFireMethod eventFireMethod)
        {
            var transitionInvoker = new EventTransitionInvoker<TState>(@event, eventFireMethod);
            return this.RequestEventFireFromEvent(transitionInvoker);
        }

        bool IEventDelegate.RequestEventFireFromEvent<TEventData>(Event<TEventData> @event, TEventData eventData, EventFireMethod eventFireMethod)
        {
            var transitionInvoker = new EventTransitionInvoker<TState, TEventData>(@event, eventFireMethod, eventData);
            return this.RequestEventFireFromEvent(transitionInvoker);
        }

        // invoker: Action which actually triggers the transition. Takes the state to transition from, and returns whether the transition was found
        private bool RequestEventFireFromEvent(ITransitionInvoker<TState> transitionInvoker)
        {
            if (this.Kernel.Synchronizer != null)
                return this.Kernel.Synchronizer.FireEvent(() => this.InvokeTransition(this.RequestEventFire, transitionInvoker), transitionInvoker.EventFireMethod);
            else
                return this.InvokeTransition(this.RequestEventFire, transitionInvoker);
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
                try
                {
                    this.Kernel.ExecutingTransition = true;
                    success = method(transitionInvoker);
                }
                catch (InternalTransitionFaultException e)
                {
                    var faultInfo = new StateMachineFaultInfo(this, e.FaultedComponent, e.InnerException, e.From, e.To, e.Event, e.Group);
                    this.Kernel.SetFault(faultInfo);
                    throw new TransitionFailedException(faultInfo);
                }
                finally
                {
                    this.Kernel.ExecutingTransition = false;
                }

                this.Kernel.FireQueuedTransitions();
            }
            finally
            {
                // Whatever happens, when we've either failed or executed everything in the transition queue,
                // the queue should end up empty.
                this.Kernel.ClearTransitionQueue();
            }

            return success;
        }

        internal override void HandleTransitionNotFound(IEvent @event, bool throwException)
        {
            this.Kernel.HandleTransitionNotFound(this.CurrentState, @event, this, throwException);

            if (throwException)
                throw new TransitionNotFoundException(this.CurrentState, @event, this);
        }

        private void OnFaulted(object sender, StateMachineFaultedEventArgs eventArgs)
        {
            this.Faulted?.Invoke(this, eventArgs);
        }

        private void OnTransition(object sender, TransitionEventArgs<TState> eventArgs)
        {
            this.Transition?.Invoke(this, eventArgs);
        }

        private void OnTransitionNotFound(object sender, TransitionNotFoundEventArgs<TState> eventArgs)
        {
            this.TransitionNotFound?.Invoke(this, eventArgs);
        }
    }
}
