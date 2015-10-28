namespace StateMechanic
{
    internal interface IEventDelegate : IStateMachine
    {
        bool RequestEventFireFromEvent(Event @event, EventFireMethod eventFireMethod);
        bool RequestEventFireFromEvent<TEventData>(Event<TEventData> @event, TEventData eventData, EventFireMethod eventFireMethod);
    }
}
