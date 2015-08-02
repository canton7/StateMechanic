using System;

namespace StateMechanic
{
    /// <summary>
    /// A state machine
    /// </summary>
    public class StateMachine : ChildStateMachine
    {
        /// <summary>
        /// Gets the fault associated with this state machine. A state machine will fault if one of its handlers throws an exception
        /// </summary>
        public StateMachineFaultInfo Fault { get { return this.InnerStateMachine.Kernel.Fault; } }

        /// <summary>
        /// Gets a value indicating whether this state machine is faulted. A state machine will fault if one of its handlers throws an exception
        /// </summary>
        public bool IsFaulted { get { return this.Fault != null; } }

        /// <summary>
        /// Gets or sets the synchronizer used by this state machine to achieve thread safety. State machines are not thread safe by default
        /// </summary>
        public IStateMachineSynchronizer Synchronizer
        {
            get { return this.InnerStateMachine.Kernel.Synchronizer; }
            set { this.InnerStateMachine.Kernel.Synchronizer = value; }
        }

        /// <summary>
        /// Event raised when a fault occurs in this state machine. A state machine will fault if one of its handlers throws an exception
        /// </summary>
        public event EventHandler<StateMachineFaultedEventArgs> Faulted;

        /// <summary>
        /// Event raised when a transition occurs in this state machine, or any of its child state machines
        /// </summary>
        public event EventHandler<TransitionEventArgs<State>> GlobalTransition;

        /// <summary>
        /// Event raised whenever an event is fired but no corresponding transition is found on this state machine or any of its child state machines
        /// </summary>
        public event EventHandler<TransitionNotFoundEventArgs<State>> GlobalTransitionNotFound;

        /// <summary>
        /// Instantiates a new instance of the <see cref="StateMachine"/> class, with the given name
        /// </summary>
        /// <param name="name">Name of this state machine</param>
        public StateMachine(string name)
            : base(name, new StateMachineKernel<State>(), null)
        {
            this.InnerStateMachine.Kernel.Faulted += this.OnFaulted;
            this.InnerStateMachine.Kernel.GlobalTransition += this.OnGlobalTransition;
            this.InnerStateMachine.Kernel.GlobalTransitionNotFound += this.OnGlobalTransitionNotFound;
        }

        /// <summary>
        /// Resets the state machine, removing any fault and returning it and any child state machines to their initial state
        /// </summary>
        public void Reset()
        {
            if (this.InnerStateMachine.Kernel.Synchronizer != null)
            {
                this.InnerStateMachine.Kernel.Synchronizer.Reset(this.Reset);
                return;
            }

            this.InnerStateMachine.Kernel.Reset();
            this.InnerStateMachine.Reset();
        }

        private void OnFaulted(object sender, StateMachineFaultedEventArgs eventArgs)
        {
            var handler = this.Faulted;
            if (handler != null)
                handler(this, eventArgs);
        }

        private void OnGlobalTransition(object sender, TransitionEventArgs<State> eventArgs)
        {
            var handler = this.GlobalTransition;
            if (handler != null)
                handler(this, eventArgs);
        }

        private void OnGlobalTransitionNotFound(object sender, TransitionNotFoundEventArgs<State> eventArgs)
        {
            var handler = this.GlobalTransitionNotFound;
            if (handler != null)
                handler(this, eventArgs);
        }
    }

    /// <summary>
    /// A state machine with per-state data
    /// </summary>
    public class StateMachine<TStateData> : ChildStateMachine<TStateData>
    {
        /// <summary>
        /// Gets the fault associated with this state machine. A state machine will fault if one of its handlers throws an exception
        /// </summary>
        public StateMachineFaultInfo Fault { get { return this.InnerStateMachine.Kernel.Fault; } }

        /// <summary>
        /// Gets a value indicating whether this state machine is faulted. A state machine will fault if one of its handlers throws an exception
        /// </summary>
        public bool IsFaulted { get { return this.Fault != null; } }

        /// <summary>
        /// Gets or sets the synchronizer used by this state machine to achieve thread safety. State machines are not thread safe by default
        /// </summary>
        public IStateMachineSynchronizer Synchronizer
        {
            get { return this.InnerStateMachine.Kernel.Synchronizer; }
            set { this.InnerStateMachine.Kernel.Synchronizer = value; }
        }

        /// <summary>
        /// Event raised when a fault occurs in this state machine. A state machine will fault if one of its handlers throws an exception
        /// </summary>
        public event EventHandler<StateMachineFaultedEventArgs> Faulted;

        /// <summary>
        /// Event raised when a transition occurs in this state machine, or any of its child state machines
        /// </summary>
        public event EventHandler<TransitionEventArgs<State<TStateData>>> GlobalTransition;

        /// <summary>
        /// Event raised whenever an event is fired but no corresponding transition is found on this state machine or any of its child state machines
        /// </summary>
        public event EventHandler<TransitionNotFoundEventArgs<State<TStateData>>> GlobalTransitionNotFound;

        /// <summary>
        /// Instantiates a new instance of the <see cref="StateMachine{TStateData}"/> class, with the given name
        /// </summary>
        /// <param name="name">Name of this state machine</param>
        public StateMachine(string name)
            : base(name, new StateMachineKernel<State<TStateData>>(), null)
        {
            this.InnerStateMachine.Kernel.Faulted += this.OnFaulted;
            this.InnerStateMachine.Kernel.GlobalTransition += this.OnGlobalTransition;
            this.InnerStateMachine.Kernel.GlobalTransitionNotFound += this.OnGlobalTransitionNotFound;
        }

        /// <summary>
        /// Resets the state machine, removing any fault and returning it and any child state machines to their initial state
        /// </summary>
        public void Reset()
        {
            if (this.InnerStateMachine.Kernel.Synchronizer != null)
            {
                this.InnerStateMachine.Kernel.Synchronizer.Reset(this.Reset);
                return;
            }

            this.InnerStateMachine.Kernel.Reset();
            this.InnerStateMachine.Reset();
        }

        private void OnFaulted(object sender, StateMachineFaultedEventArgs eventArgs)
        {
            var handler = this.Faulted;
            if (handler != null)
                handler(this, eventArgs);
        }

        private void OnGlobalTransition(object sender, TransitionEventArgs<State<TStateData>> eventArgs)
        {
            var handler = this.GlobalTransition;
            if (handler != null)
                handler(this, eventArgs);
        }

        private void OnGlobalTransitionNotFound(object sender, TransitionNotFoundEventArgs<State<TStateData>> eventArgs)
        {
            var handler = this.GlobalTransitionNotFound;
            if (handler != null)
                handler(this, eventArgs);
        }
    }
}
