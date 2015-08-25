namespace StateMechanic
{
    internal static class Transition
    {
        internal static Transition<TState> Create<TState>(TState from, TState to, Event @event, ITransitionDelegate<TState> transitionDelegate) where TState : class, IState<TState>
        {
            return new Transition<TState>(new TransitionInner<TState, Event, TransitionInfo<TState>>(from, to, @event, transitionDelegate, isInnerTransition: false));
        }

        internal static Transition<TState> CreateInner<TState>(TState fromAndTo, Event @event, ITransitionDelegate<TState> transitionDelegate) where TState : class, IState<TState>
        {
            return new Transition<TState>(new TransitionInner<TState, Event, TransitionInfo<TState>>(fromAndTo, fromAndTo, @event, transitionDelegate, isInnerTransition: true));
        }

        internal static Transition<TState, TEventData> Create<TState, TEventData>(TState from, TState to, Event<TEventData> @event, ITransitionDelegate<TState> transitionDelegate) where TState : class, IState<TState>
        {
            return new Transition<TState, TEventData>(new TransitionInner<TState, Event<TEventData>, TransitionInfo<TState, TEventData>>(from, to, @event, transitionDelegate, isInnerTransition: false));
        }

        internal static Transition<TState, TEventData> CreateInner<TState, TEventData>(TState fromAndTo, Event<TEventData> @event, ITransitionDelegate<TState> transitionDelegate) where TState : class, IState<TState>
        {
            return new Transition<TState, TEventData>(new TransitionInner<TState, Event<TEventData>, TransitionInfo<TState, TEventData>>(fromAndTo, fromAndTo, @event, transitionDelegate, isInnerTransition: true));
        }
    }
}
