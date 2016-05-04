namespace StateMechanic
{
    internal interface IInvokableTransition
    {
        bool TryInvoke(EventFireMethod eventFireMethod);
    }

    internal interface IInvokableTransition<TEventData>
    {
        bool TryInvoke(TEventData eventData, EventFireMethod eventFireMethod);
    }
}
