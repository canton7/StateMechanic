namespace StateMechanic
{
    internal interface IInvokableTransition
    {
        bool TryInvoke();
    }

    internal interface IInvokableTransition<TEventData>
    {
        bool TryInvoke(TEventData eventData);
    }
}
