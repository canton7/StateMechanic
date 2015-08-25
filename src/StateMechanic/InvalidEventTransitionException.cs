using System;

namespace StateMechanic
{
    /// <summary>
    /// Exception thrown when a transition could not be created from a state on an event, because the state and event do not belong to the same state machine
    /// </summary>
    public class InvalidEventTransitionException : Exception
    {
        /// <summary>
        /// Gets the state from which the transition could not be created
        /// </summary>
        public IState From { get; }

        /// <summary>
        /// Gets the event on which the transition could not be created
        /// </summary>
        public IEvent Event { get; }

        internal InvalidEventTransitionException(IState from, IEvent @event)
            : base(String.Format("Unable to create transition from state {0} on event {1}, as state {0} does not belong to the same state machine as event {1}, or to a child state machine of event {1}", from.Name, @event.Name))
        {
            this.From = from;
            this.Event = @event;
        }
    }
}
