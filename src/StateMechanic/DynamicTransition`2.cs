using System;

namespace StateMechanic
{
    public class DynamicTransition<TState, TEventData> : IDynamicTransition<TState>, IInvokableTransition<TEventData> where TState : class, IState
    {
        private readonly DynamicTransitionInner<TState, Event<TEventData>, TransitionInfo<TState, TEventData>, DynamicSelectorInfo<TState, TEventData>> innerTransition;

        public TState From => this.innerTransition.From;
        public Event<TEventData> Event => this.innerTransition.Event;
        IEvent IDynamicTransition<TState>.Event => this.innerTransition.Event;

        public Func<DynamicSelectorInfo<TState, TEventData>, TState> StateSelector
        {
            get { return this.innerTransition.StateSelector; }
            set { this.innerTransition.StateSelector = value; }
        }

        public Action<TransitionInfo<TState, TEventData>> Handler
        {
            get { return this.innerTransition.Handler; }
            set { this.innerTransition.Handler = value; }
        }

        internal DynamicTransition(TState from, Event<TEventData> @event, Func<DynamicSelectorInfo<TState, TEventData>, TState> stateSelector, ITransitionDelegate<TState> transitionDelegate)
        {
            this.innerTransition = new DynamicTransitionInner<TState, Event<TEventData>, TransitionInfo<TState, TEventData>, DynamicSelectorInfo<TState, TEventData>>(from, @event, stateSelector, transitionDelegate);
        }

        public DynamicTransition<TState, TEventData> WithHandler(Action<TransitionInfo<TState, TEventData>> handler)
        {
            this.Handler = handler;
            return this;
        }

        bool IInvokableTransition<TEventData>.TryInvoke(TEventData eventData)
        {
            var dynamicTransitionInfo = new DynamicSelectorInfo<TState, TEventData>(this.innerTransition.From, this.innerTransition.Event, eventData);
            var to = this.innerTransition.FindToState(dynamicTransitionInfo);
            if (to == null)
                return false;
            this.innerTransition.Invoke(to, new TransitionInfo<TState, TEventData>(this.innerTransition.From, to, this.innerTransition.Event, eventData, false));
            return true;
        }
    }
}
