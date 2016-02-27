namespace StateMechanic
{
    public class DynamicSelectorInfo<TState>
    {
        public TState From { get; }
        public Event Event { get; }

        internal DynamicSelectorInfo(TState from, Event @event)
        {
            this.From = from;
            this.Event = @event;
        }
    }
}
