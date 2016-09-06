using System;

namespace StateMechanic
{
    internal class TransitionInner<TState, TEvent, TTransitionInfo> : ITransitionInner<TState, TEvent, TTransitionInfo>
        where TState : StateBase<TState>, new()
        where TEvent : IEvent
        where TTransitionInfo : ITransitionInfo<TState>
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

            this.From = from;
            this.To = to;
            this.Event = @event;
            this.transitionDelegate = transitionDelegate;
            this.IsInnerTransition = isInnerTransition;
        }

        public bool TryInvoke(TTransitionInfo transitionInfo)
        {
            if (!this.From.CanTransition(this.Event, this.To))
                return false;

            var guard = this.Guard;
            if (guard != null && !guard(transitionInfo))
                return false;

            this.transitionDelegate.CoordinateTransition(transitionInfo, this.Handler);

            return true;
        }
    }
}
