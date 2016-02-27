using System;

namespace StateMechanic
{
    public class DynamicTransition<TState> : IDynamicTransition<TState>, IInvokableTransition where TState : class, IState
    {
        private readonly DynamicTransitionInner<TState, Event, TransitionInfo<TState>, DynamicSelectorInfo<TState>> innerTransition;

        public TState From => this.innerTransition.From;
        public Event Event => this.innerTransition.Event;
        IEvent IDynamicTransition<TState>.Event => this.innerTransition.Event;

        public Func<DynamicSelectorInfo<TState>, TState> StateSelector
        {
            get { return this.innerTransition.StateSelector; }
            set { this.innerTransition.StateSelector = value; }
        }

        public Action<TransitionInfo<TState>> Handler
        {
            get { return this.innerTransition.Handler; }
            set { this.innerTransition.Handler = value; }
        }

        internal DynamicTransition(TState from, Event @event, Func<DynamicSelectorInfo<TState>, TState> stateSelector, ITransitionDelegate<TState> transitionDelegate)
        {
            this.innerTransition = new DynamicTransitionInner<TState, Event, TransitionInfo<TState>, DynamicSelectorInfo<TState>>(from, @event, stateSelector, transitionDelegate);
        }

        public DynamicTransition<TState> WithHandler(Action<TransitionInfo<TState>> handler)
        {
            this.Handler = handler;
            return this;
        }

        bool IInvokableTransition.TryInvoke()
        {
            var dynamicTransitionInfo = new DynamicSelectorInfo<TState>(this.innerTransition.From, this.innerTransition.Event);
            var to = this.innerTransition.FindToState(dynamicTransitionInfo);
            if (to == null)
                return false;
            this.innerTransition.Invoke(to, new TransitionInfo<TState>(this.innerTransition.From, to, this.innerTransition.Event, false));
            return true;
        }
    }
}
