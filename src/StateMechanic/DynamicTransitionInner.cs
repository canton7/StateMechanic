using System;

namespace StateMechanic
{
    internal class DynamicTransitionInner<TState, TEvent, TTransitionInfo, TDynamicSelectorInfo>
        where TState : StateBase<TState>, new()
        where TEvent : IEvent
        where TTransitionInfo : ITransitionInfo<TState>
    {
        private readonly ITransitionDelegate<TState> transitionDelegate;

        public TState From { get; }
        public TEvent Event { get; }

        private Func<TDynamicSelectorInfo, TState> stateSelector;
        public Func<TDynamicSelectorInfo, TState> StateSelector
        {
            get { return this.stateSelector; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                this.stateSelector = value;
            }
        }

        public Action<TTransitionInfo> Handler { get; set; }

        public DynamicTransitionInner(TState from, TEvent @event, Func<TDynamicSelectorInfo, TState> stateSelector, ITransitionDelegate<TState> transitionDelegate)
        {
            this.From = from;
            this.Event = @event;
            this.stateSelector = stateSelector;
            this.transitionDelegate = transitionDelegate;
        }

        public TState FindToState(TDynamicSelectorInfo transitionInfo)
        {
            var to = this.StateSelector(transitionInfo);
            if (to == null)
                return null;

            if (this.From.ParentStateMachine != to.ParentStateMachine)
                throw new InvalidStateTransitionException(this.From, to);

            return to;
        }

        public bool TryInvoke(TTransitionInfo transitionInfo)
        {
            if (!this.From.CanTransition(this.Event, transitionInfo.To))
                return false;

            this.transitionDelegate.CoordinateTransition(transitionInfo, this.Handler);
            return true;
        }
    }
}
