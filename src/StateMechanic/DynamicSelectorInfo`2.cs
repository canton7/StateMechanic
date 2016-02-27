namespace StateMechanic
{
    public class DynamicSelectorInfo<TState, TEventData>
    {
        public TState From { get; }
        public Event<TEventData> Event { get; }
        public TEventData EventData { get; }

        public DynamicSelectorInfo(TState from, Event<TEventData> @event, TEventData eventData)
        {
            this.From = from;
            this.Event = @event;
            this.EventData = eventData;
        }
    }
}
