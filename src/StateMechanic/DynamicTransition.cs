





using System;

namespace StateMechanic
{

    /// <summary>
    /// A transition from one state to another, triggered by an event, where the destination state is determined by a user-supplied callback
    /// </summary>
    /// <typeparam name="TState">Type of state which this transition is between</typeparam>

    public class DynamicTransition<TState> : ITransition<TState>, IInvokableTransition
        where TState : StateBase<TState>, new()
    {
        private readonly ITransitionDelegate<TState> transitionDelegate;

        /// <summary>
        /// Gets the state this transition is from
        /// </summary>
        public TState From { get; }

        TState ITransition<TState>.To => null;

        /// <summary>
        /// Gets the event which triggers this transition
        /// </summary>
        public Event Event { get; }
        IEvent ITransition<TState>.Event => this.Event;

        bool ITransition<TState>.IsDynamicTransition => true;
        bool ITransition<TState>.IsInnerTransition => false;
        bool ITransition<TState>.HasGuard => false;
        bool ITransition.WillAlwaysOccur => false;

        private Func<DynamicSelectorInfo<TState>, TState> _stateSelector;
        /// <summary>
        /// Gets or sets the callback which determines which state is transitioned to
        /// </summary>
        public Func<DynamicSelectorInfo<TState>, TState> StateSelector
        {
            get { return this._stateSelector; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                this._stateSelector = value;
            }
        }

        /// <summary>
        /// Gets or sets a method which is invoked whenever this transition occurs
        /// </summary>
        public Action<TransitionInfo<TState>> Handler { get; set; }

        internal DynamicTransition(TState from, Event @event, Func<DynamicSelectorInfo<TState>, TState> stateSelector, ITransitionDelegate<TState> transitionDelegate)
        {
            this.From = from;
            this.Event = @event;
            this._stateSelector = stateSelector;
            this.transitionDelegate = transitionDelegate;
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
            var selectorInfo = new DynamicSelectorInfo<TState>(this.From, this.Event);
            var to = this.FindToState(selectorInfo);
            if (to == null)
                return false;

            if (!this.From.CanTransition(this.Event, to, null))
                return false;

            var transitionInfo = new TransitionInfo<TState>(this.From, to, this.Event, false, eventFireMethod);
            this.transitionDelegate.CoordinateTransition(transitionInfo, this.Handler);
            return true;
        }

        private TState FindToState(DynamicSelectorInfo<TState> selectorInfo)
        {
            var to = this.StateSelector(selectorInfo);
            if (to == null)
                return null;

            if (this.From.ParentStateMachine != to.ParentStateMachine)
                throw new InvalidStateTransitionException(this.From, to);

            return to;
        }
    }



    /// <summary>
    /// A transition from one state to another, triggered by an event, where the destination state is determined by a user-supplied callback
    /// </summary>
    /// <typeparam name="TState">Type of state which this transition is between</typeparam>

    /// <typeparam name="TEventData">Type of event data associated with the event which triggers this transition</typeparam>

    public class DynamicTransition<TState, TEventData> : ITransition<TState>, IInvokableTransition<TEventData>
        where TState : StateBase<TState>, new()
    {
        private readonly ITransitionDelegate<TState> transitionDelegate;

        /// <summary>
        /// Gets the state this transition is from
        /// </summary>
        public TState From { get; }

        TState ITransition<TState>.To => null;

        /// <summary>
        /// Gets the event which triggers this transition
        /// </summary>
        public Event<TEventData> Event { get; }
        IEvent ITransition<TState>.Event => this.Event;

        bool ITransition<TState>.IsDynamicTransition => true;
        bool ITransition<TState>.IsInnerTransition => false;
        bool ITransition<TState>.HasGuard => false;
        bool ITransition.WillAlwaysOccur => false;

        private Func<DynamicSelectorInfo<TState, TEventData>, TState> _stateSelector;
        /// <summary>
        /// Gets or sets the callback which determines which state is transitioned to
        /// </summary>
        public Func<DynamicSelectorInfo<TState, TEventData>, TState> StateSelector
        {
            get { return this._stateSelector; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                this._stateSelector = value;
            }
        }

        /// <summary>
        /// Gets or sets a method which is invoked whenever this transition occurs
        /// </summary>
        public Action<TransitionInfo<TState, TEventData>> Handler { get; set; }

        internal DynamicTransition(TState from, Event<TEventData> @event, Func<DynamicSelectorInfo<TState, TEventData>, TState> stateSelector, ITransitionDelegate<TState> transitionDelegate)
        {
            this.From = from;
            this.Event = @event;
            this._stateSelector = stateSelector;
            this.transitionDelegate = transitionDelegate;
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

        bool IInvokableTransition<TEventData>.TryInvoke(TEventData eventData, EventFireMethod eventFireMethod)
        {
            var selectorInfo = new DynamicSelectorInfo<TState, TEventData>(this.From, this.Event, eventData);
            var to = this.FindToState(selectorInfo);
            if (to == null)
                return false;

            if (!this.From.CanTransition(this.Event, to, eventData))
                return false;

            var transitionInfo = new TransitionInfo<TState, TEventData>(this.From, to, this.Event, eventData, false, eventFireMethod);
            this.transitionDelegate.CoordinateTransition(transitionInfo, this.Handler);
            return true;
        }

        private TState FindToState(DynamicSelectorInfo<TState, TEventData> selectorInfo)
        {
            var to = this.StateSelector(selectorInfo);
            if (to == null)
                return null;

            if (this.From.ParentStateMachine != to.ParentStateMachine)
                throw new InvalidStateTransitionException(this.From, to);

            return to;
        }
    }

}

