using System;

namespace StateMechanic
{
    // Oh the hoops we jump through to have Transition<T> public...
    internal interface ITransitionInner<TState, TEvent, TTransitionInfo> where TState : class, IState
    {
        TState From { get; }
        TState To { get; }
        TEvent Event { get; }
        bool IsInnerTransition { get; }
        Action<TTransitionInfo> Handler { get; set; }
        Func<TTransitionInfo, bool> Guard { get; set; }
        bool TryInvoke(TTransitionInfo transitionInfo);
    }

    internal class TransitionInner<TState, TEvent, TTransitionInfo> : ITransitionInner<TState, TEvent, TTransitionInfo> where TState : class, IState<TState> where TEvent : IEvent
    {
        public TState From { get; }
        public TState To { get; }
        public TEvent Event { get; }
        public bool IsInnerTransition { get; }
        private readonly ITransitionDelegate<TState> transitionDelegate;

        public Action<TTransitionInfo> Handler { get; set; }
        public Func<TTransitionInfo, bool> Guard { get; set; }

        public TransitionInner(TState from, TState to, TEvent @event, ITransitionDelegate<TState> transitionDelegate, bool isInnerTransition)
        {
            if (from.ParentStateMachine != to.ParentStateMachine)
                throw new InvalidStateTransitionException(from, to);

            if (from.ParentStateMachine != @event.ParentStateMachine && !from.ParentStateMachine.IsChildOf(@event.ParentStateMachine))
                throw new InvalidEventTransitionException(from, @event);

            this.From = from;
            this.To = to;
            this.Event = @event;
            this.transitionDelegate = transitionDelegate;
            this.IsInnerTransition = isInnerTransition;
        }

        public bool TryInvoke(TTransitionInfo transitionInfo)
        {
            var guard = this.Guard;
            if (guard != null && !guard(transitionInfo))
                return false;

            this.transitionDelegate.CoordinateTransition(this.From, this.To, this.Event, this.IsInnerTransition, this.Handler == null ? (Action)null : () => this.Handler(transitionInfo));

            return true;
        }
    }
}
