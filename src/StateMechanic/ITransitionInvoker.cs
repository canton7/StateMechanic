namespace StateMechanic
{
    internal interface ITransitionInvoker<TState> where TState : IState
    {
        // This is a hack to work around an issue with Mono's full AOT, which crashes if you have a generic
        // method with a constrained type, and an enum is retured from that type.
        // See https://bugzilla.xamarin.com/show_bug.cgi?id=52600
        // It's been fixed in mono's master, but this will take time to reach things like Xamarin.iOS.
        // For now, then, return an int (and cast to an EventFireInfo), rather than returning an EventFireInfo
        // directly.
        int EventFireMethodInt { get; }
        IEvent Event { get; }
        bool TryInvoke(TState sourceState);
    }
}
