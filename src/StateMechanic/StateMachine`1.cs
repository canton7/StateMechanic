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
