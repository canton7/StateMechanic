namespace StateMechanic
{
    internal interface IEventDelegate
    {
        bool RequestEventFireFromEvent(Event @event, EventFireMethod eventFireMethod);
        bool RequestEventFireFromEvent<TEventData>(Event<TEventData> @event, TEventData eventData, EventFireMethod eventFireMethod);
    }
}
