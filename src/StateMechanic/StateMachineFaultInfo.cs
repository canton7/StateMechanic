using System;

namespace StateMechanic
{
    /// <summary>
    /// Indicates which component of the transition threw an exception
    /// </summary>
    public enum FaultedComponent
    {
        /// <summary>
        /// A state exit handler threw an excpetion
        /// </summary>
        ExitHandler,

        /// <summary>
        /// A StateGroup exit handler threw an exception
        /// </summary>
        GroupExitHandler,

        /// <summary>
        /// A transition handler threw an exception
        /// </summary>
        TransitionHandler,

        /// <summary>
        /// A StateGroup entry handler threw an exception
        /// </summary>
        GroupEntryHandler,

        /// <summary>
        /// A state entry handler threw an exception
        /// </summary>
        EntryHandler,
    }

    /// <summary>
    /// Information about a state machine fault (an exception thrown from a guard/entry/exit/transition handler)
    /// </summary>
    public class StateMachineFaultInfo
    {
        /// <summary>
        /// Gets the state machine which faulted
        /// </summary>
        public IStateMachine StateMachine { get; private set; }

        /// <summary>
        /// Gets the component which threw the exception
        /// </summary>
        public FaultedComponent FaultedComponent { get; private set; }

        /// <summary>
        /// Gets the exception which was thrown
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Gets the state which was being transitioned from
        /// </summary>
        public IState From { get; private set; }

        /// <summary>
        /// Gets the state which was being transitioned to
        /// </summary>
        public IState To { get; private set; }

        /// <summary>
        /// Gets the event which triggered this transition
        /// </summary>
        public IEvent Event { get; private set; }

        /// <summary>
        /// If this fault is due to an exception in a group entry/exit handler, the group at fault
        /// </summary>
        public IStateGroup Group { get; private set; }

        internal StateMachineFaultInfo(IStateMachine stateMachine, FaultedComponent faultedComponent, Exception exception, IState from, IState to, IEvent @event, IStateGroup group = null)
        {
            this.StateMachine = stateMachine;
            this.FaultedComponent = faultedComponent;
            this.Exception = exception;
            this.From = from;
            this.To = to;
            this.Event = @event;
            this.Group = group;
        }
    }
}
