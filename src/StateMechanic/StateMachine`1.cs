using System;

namespace StateMechanic
{
    /// <summary>
    /// A state machine
    /// </summary>
    public class StateMachine<TState> : ChildStateMachine<TState> where TState : StateBase<TState>, new()
    {
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

            IStateMachine<TState> stateMachine = this;

            foreach (var state in this.serializer.Deserialize(this, serialized))
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
