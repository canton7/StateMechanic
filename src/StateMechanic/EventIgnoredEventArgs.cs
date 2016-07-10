using System;

namespace StateMechanic
{
    /// <summary>
    /// EventArgs for events indicating that an event was raised, but was ignored
    /// </summary>
    /// <typeparam name="TState">Type of state</typeparam>
    public class EventIgnoredEventArgs<TState> : EventArgs
    {
        /// <summary>
        /// Gets the state the state machine was in when the event was ignored
        /// </summary>
        public TState State { get; }

        /// <summary>
        /// Gets the event which was ignored
        /// </summary>
        public IEvent Event { get; }

        /// <summary>
        /// Gets the method used to fire the event which was ignored
        /// </summary>
        public EventFireMethod EventFireMethod { get; }

        internal EventIgnoredEventArgs(TState state, IEvent @event, EventFireMethod eventFireMethod)
        {
            this.State = state;
            this.Event = @event;
            this.EventFireMethod = eventFireMethod;
        }
    }
}
