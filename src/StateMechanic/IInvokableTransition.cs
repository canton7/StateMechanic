namespace StateMechanic
{
    internal interface IInvokableTransition<TEventData>
    {
        bool TryInvoke(TEventData eventData);
    }

    // Yes, using IInvocableTransition<object> for this is ugly. But it saves a delegate creation on event fire, in Event.{Try,}Fire
    internal interface IInvokableTransition : IInvokableTransition<object> { }
}
