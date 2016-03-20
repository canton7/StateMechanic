using System;

namespace StateMechanic
{
    /// <summary>
    /// A transition from one state to another, triggered by an event, where the destination state is determined by a user-supplied callback
    /// </summary>
    /// <typeparam name="TState">Type of state which this transition is between</typeparam>
    /// /// <typeparam name="TEventData">Type of event data associated with the event which triggers this transition</typeparam>
    public class DynamicTransition<TState, TEventData> : ITransition<TState>, IInvokableTransition<TEventData> where TState : class, IState
    {
        private readonly DynamicTransitionInner<TState, Event<TEventData>, TransitionInfo<TState, TEventData>, DynamicSelectorInfo<TState, TEventData>> innerTransition;

        /// <summary>
        /// Gets the state this transition is from
        /// </summary>
        public TState From => this.innerTransition.From;

        TState ITransition<TState>.To => null;

        /// <summary>
        /// Gets the event which triggers this transition
        /// </summary>
        public Event<TEventData> Event => this.innerTransition.Event;
        IEvent ITransition<TState>.Event => this.innerTransition.Event;

        bool ITransition<TState>.IsDynamicTransition => true;
        bool ITransition<TState>.IsInnerTransition => false;
        bool ITransition<TState>.HasGuard => false;

        /// <summary>
        /// Gets or sets the callback which determines which state is transitioned to
        /// </summary>
        public Func<DynamicSelectorInfo<TState, TEventData>, TState> StateSelector
        {
            get { return this.innerTransition.StateSelector; }
            set { this.innerTransition.StateSelector = value; }
        }

        /// <summary>
        /// Gets or sets a method which is invoked whenever this transition occurs
        /// </summary>
        public Action<TransitionInfo<TState, TEventData>> Handler
        {
            get { return this.innerTransition.Handler; }
            set { this.innerTransition.Handler = value; }
        }

        internal DynamicTransition(TState from, Event<TEventData> @event, Func<DynamicSelectorInfo<TState, TEventData>, TState> stateSelector, ITransitionDelegate<TState> transitionDelegate)
        {
            this.innerTransition = new DynamicTransitionInner<TState, Event<TEventData>, TransitionInfo<TState, TEventData>, DynamicSelectorInfo<TState, TEventData>>(from, @event, stateSelector, transitionDelegate);
        }

        /// <summary>
        /// Sets a method which is invoked whenever this transition occurs
        /// </summary>
        /// <param name="handler">Method which is invoked whenever this transition occurs</param>
        /// <returns>This transition, for method chaining</returns>
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
