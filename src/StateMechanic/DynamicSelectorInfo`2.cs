namespace StateMechanic
{
    /// <summary>
    /// Contains information on a dynamic transition, passed to the transition's state selector
    /// </summary>
    /// <typeparam name="TState">Type of state</typeparam>
    /// <typeparam name="TEventData">Type of event data</typeparam>
    public class DynamicSelectorInfo<TState, TEventData>
    {
        /// <summary>
        /// Gets the state this transition is from
        /// </summary>
        public TState From { get; }

        /// <summary>
        /// Gets the event which triggered this transition
        /// </summary>
        public Event<TEventData> Event { get; }

        /// <summary>
        /// Gets the event data which was passed when the event was fired
        /// </summary>
        public TEventData EventData { get; }

        internal DynamicSelectorInfo(TState from, Event<TEventData> @event, TEventData eventData)
        {
            this.From = from;
            this.Event = @event;
            this.EventData = eventData;
        }
    }
}
