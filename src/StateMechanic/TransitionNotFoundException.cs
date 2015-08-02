using System;

namespace StateMechanic
{
    /// <summary>
    /// Indicates that no transition exists from the current state on the given event
    /// </summary>
    public class TransitionNotFoundException : Exception
    {
        /// <summary>
        /// Gets the current state, from which a transtion was requested
        /// </summary>
        public IState From { get; private set; }

        /// <summary>
        /// Gets the event which was fired
        /// </summary>
        public IEvent Event { get; private set; }

        /// <summary>
        /// Gets the state machine associated with the failed transition
        /// </summary>
        public IStateMachine StateMachine { get; private set; }

        internal TransitionNotFoundException(IState from, IEvent @event, IStateMachine stateMachine)
            : base(String.Format("Could not find a transition which we could invoke from state {0} (or any of its children) on event {1}", from.Name, @event.Name))
        {
            this.From = from;
            this.Event = @event;
            this.StateMachine = stateMachine;
        }
    }
}
