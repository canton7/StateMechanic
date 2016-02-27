namespace StateMechanic
{
    public class DynamicSelectorInfo<TState, TEventData>
    {
        TState From { get; }
        Event<TEventData> Event { get; }
        TEventData EventData { get; }

        public DynamicSelectorInfo(TState from, Event<TEventData> @event, TEventData eventData)
        {
            this.From = from;
            this.Event = @event;
            this.EventData = eventData;
        }
    }
}
