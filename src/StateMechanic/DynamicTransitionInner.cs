using System;

namespace StateMechanic
{
    internal class DynamicTransitionInner<TState, TEvent, TTransitionInfo, TDynamicSelectorInfo>
        where TState : StateBase<TState>, new()
        where TEvent : IEvent
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

        public bool TryInvoke(TState to, TTransitionInfo transitionInfo)
        {
            if (!this.From.CanTransition(this.Event, to))
                return false;

            this.transitionDelegate.CoordinateTransition(this.From, to, this.Event, false, this.Handler, transitionInfo);
            return true;
        }
    }
}
