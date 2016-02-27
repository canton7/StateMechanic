namespace StateMechanic
{
    public class DynamicSelectorInfo<TState>
    {
        TState From { get; }
        Event Event { get; }

        internal DynamicSelectorInfo(TState from, Event @event)
        {
            this.From = from;
            this.Event = @event;
        }
    }
}
