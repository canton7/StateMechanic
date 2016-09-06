namespace StateMechanic
{
    internal interface IInvokableTransition : ITransition
    {
        bool TryInvoke(EventFireMethod eventFireMethod);
    }

    internal interface IInvokableTransition<TEventData> : ITransition
    {
        bool TryInvoke(TEventData eventData, EventFireMethod eventFireMethod);
    }
}
