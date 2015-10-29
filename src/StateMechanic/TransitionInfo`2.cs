namespace StateMechanic
{
    /// <summary>
    /// Contains information on the currently-executing transition
    /// </summary>
    /// <typeparam name="TState">Type of state</typeparam>
    /// <typeparam name="TEventData">Type of event data associated with the event</typeparam>
    public class TransitionInfo<TState, TEventData>
    {
        /// <summary>
        /// Gets the state this transition is from
        /// </summary>
        public TState From { get; }

        /// <summary>
        /// Gets the state this transition is to
        /// </summary>
        public TState To { get; }

        /// <summary>
        /// Gets the event which triggered this transition
        /// </summary>
        public Event<TEventData> Event { get; }

        /// <summary>
        /// Gets the event data which was passed when the event was fired
        /// </summary>
        public TEventData EventData { get; }

        /// <summary>
        /// Gets a value indicating whether this is an inner self transition, i.e. whether entry/exit handler are not executed
        /// </summary>
        public bool IsInnerTransition { get; }

        internal TransitionInfo(TState from, TState to, Event<TEventData> @event, TEventData eventData, bool isInnerTransition)
        {
            this.From = from;
            this.To = to;
            this.Event = @event;
            this.EventData = eventData;
            this.IsInnerTransition = isInnerTransition;
        }
    }
}
