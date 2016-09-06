using System;

namespace StateMechanic
{
    /// <summary>
    /// A transition from one state to another, triggered by an event, where the destination state is determined by a user-supplied callback
    /// </summary>
    /// <typeparam name="TState">Type of state which this transition is between</typeparam>
    public class DynamicTransition<TState> : ITransition<TState>, IInvokableTransition where TState : StateBase<TState>, new()
    {
        private readonly DynamicTransitionInner<TState, Event, TransitionInfo<TState>, DynamicSelectorInfo<TState>> innerTransition;

        /// <summary>
        /// Gets the state this transition is from
        /// </summary>
        public TState From => this.innerTransition.From;

        TState ITransition<TState>.To => null;

        /// <summary>
        /// Gets the event which triggers this transition
        /// </summary>
        public Event Event => this.innerTransition.Event;
        IEvent ITransition<TState>.Event => this.innerTransition.Event;

        bool ITransition<TState>.IsDynamicTransition => true;
        bool ITransition<TState>.IsInnerTransition => false;
        bool ITransition<TState>.HasGuard => false;
        bool ITransition.WillAlwaysOccur => false;

        /// <summary>
        /// Gets or sets the callback which determines which state is transitioned to
        /// </summary>
        public Func<DynamicSelectorInfo<TState>, TState> StateSelector
        {
            get { return this.innerTransition.StateSelector; }
            set { this.innerTransition.StateSelector = value; }
        }

        /// <summary>
        /// Gets or sets a method which is invoked whenever this transition occurs
        /// </summary>
        public Action<TransitionInfo<TState>> Handler
        {
            get { return this.innerTransition.Handler; }
            set { this.innerTransition.Handler = value; }
        }

        internal DynamicTransition(TState from, Event @event, Func<DynamicSelectorInfo<TState>, TState> stateSelector, ITransitionDelegate<TState> transitionDelegate)
        {
            this.innerTransition = new DynamicTransitionInner<TState, Event, TransitionInfo<TState>, DynamicSelectorInfo<TState>>(from, @event, stateSelector, transitionDelegate);
        }

        /// <summary>
        /// Sets a method which is invoked whenever this transition occurs
        /// </summary>
        /// <param name="handler">Method which is invoked whenever this transition occurs</param>
        /// <returns>This transition, for method chaining</returns>
        public DynamicTransition<TState> WithHandler(Action<TransitionInfo<TState>> handler)
        {
            this.Handler = handler;
            return this;
        }

        bool IInvokableTransition.TryInvoke(EventFireMethod eventFireMethod)
        {
            var dynamicTransitionInfo = new DynamicSelectorInfo<TState>(this.innerTransition.From, this.innerTransition.Event);
            var to = this.innerTransition.FindToState(dynamicTransitionInfo);
            if (to == null)
                return false;

            return this.innerTransition.TryInvoke(new TransitionInfo<TState>(this.innerTransition.From, to, this.innerTransition.Event, false, eventFireMethod));
        }
    }
}
