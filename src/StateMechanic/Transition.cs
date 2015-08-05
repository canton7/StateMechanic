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
        public TState From { get; private set; }
        public TState To { get; private set; }
        public TEvent Event { get; private set; }
        public bool IsInnerTransition { get; private set; }
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
            if (this.Guard != null)
            {
                if (!this.Guard(transitionInfo))
                    return false;
            }

            this.transitionDelegate.CoordinateTransition(this.From, this.To, this.Event, this.IsInnerTransition, this.Handler == null ? (Action)null : () => this.Handler(transitionInfo));

            return true;
        }
    }

    /// <summary>
    /// A transition from one state to another, triggered by an event
    /// </summary>
    /// <typeparam name="TState">Type of state which this transition is between</typeparam>
    public class Transition<TState> : ITransition<TState>, IInvokableTransition where TState : class, IState
    {
        private readonly ITransitionInner<TState, Event, TransitionInfo<TState>> innerTransition;

        /// <summary>
        /// Gets the state this transition is from
        /// </summary>
        public TState From { get { return this.innerTransition.From; } }

        /// <summary>
        /// Gets the state this transition to
        /// </summary>
        public TState To { get { return this.innerTransition.To; } }

        /// <summary>
        /// Gets the event which triggers this transition
        /// </summary>
        public Event Event { get { return this.innerTransition.Event; } }
        IEvent ITransition<TState>.Event { get { return this.innerTransition.Event; } }

        /// <summary>
        /// Gets a value indicating whether this transition is an inner transition, i.e. whether the <see cref="From"/> and <see cref="To"/> states are the same, and no exit/entry handles are invoked
        /// </summary>
        public bool IsInnerTransition { get { return this.innerTransition.IsInnerTransition; } }

        /// <summary>
        /// Gets or sets a method which is invoked whenever this transition occurs
        /// </summary>
        public Action<TransitionInfo<TState>> Handler
        {
            get { return this.innerTransition.Handler; }
            set { this.innerTransition.Handler = value; }
        }

        /// <summary>
        /// Gets or sets a method which is invoked before this transition occurs, and can prevent the transition from occuring
        /// </summary>
        public Func<TransitionInfo<TState>, bool> Guard
        {
            get { return this.innerTransition.Guard; }
            set { this.innerTransition.Guard = value; }
        }

        internal Transition(ITransitionInner<TState, Event, TransitionInfo<TState>> innerTransition)
        {
            this.innerTransition = innerTransition;
        }

        /// <summary>
        /// Sets a method which is invoked whenever this transition occurs
        /// </summary>
        /// <param name="handler">Method which is invoked whenever this transition occurs</param>
        /// <returns>This transition, for method chaining</returns>
        public Transition<TState> WithHandler(Action<TransitionInfo<TState>> handler)
        {
            this.Handler = handler;
            return this;
        }

        /// <summary>
        /// Sets a method which is invoked before this transition occurs, and can prevent the transition from occuring
        /// </summary>
        /// <param name="guard">method which is invoked before this transition occurs, and can prevent the transition from occuring</param>
        /// <returns>This transition, for method chaining</returns>
        public Transition<TState> WithGuard(Func<TransitionInfo<TState>, bool> guard)
        {
            this.Guard = guard;
            return this;
        }

        bool IInvokableTransition.TryInvoke()
        {
            var transitionInfo = new TransitionInfo<TState>(this.From, this.To, this.Event, this.IsInnerTransition);
            return this.innerTransition.TryInvoke(transitionInfo);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object</returns>
        public override string ToString()
        {
            return String.Format("<Transition From={0} To={1} Event={2}{3}>", this.From.Name, this.To.Name, this.Event.Name, this.IsInnerTransition ? " IsInnerTransition" : "");
        }
    }

    /// <summary>
    /// A transition from one state to another, triggered by an event
    /// </summary>
    /// <typeparam name="TState">Type of state which this transition is between</typeparam>
    /// <typeparam name="TEventData">Type of event data associated with the event which triggers this transition</typeparam>
    public class Transition<TState, TEventData> : ITransition<TState>, IInvokableTransition<TEventData> where TState : class, IState
    {
        private readonly ITransitionInner<TState, Event<TEventData>, TransitionInfo<TState, TEventData>> innerTransition;

        /// <summary>
        /// Gets the state this transition is from
        /// </summary>
        public TState From { get { return this.innerTransition.From; } }

        /// <summary>
        /// Gets the state this transition to
        /// </summary>
        public TState To { get { return this.innerTransition.To; } }

        /// <summary>
        /// Gets the event which triggers this transition
        /// </summary>
        public Event<TEventData> Event { get { return this.innerTransition.Event; } }
        IEvent ITransition<TState>.Event { get { return this.innerTransition.Event; } }

        /// <summary>
        /// Gets a value indicating whether this transition is an inner transition, i.e. whether the <see cref="From"/> and <see cref="To"/> states are the same, and no exit/entry handles are invoked
        /// </summary>
        public bool IsInnerTransition { get { return this.innerTransition.IsInnerTransition; } }

        /// <summary>
        /// Gets or sets a method which is invoked whenever this transition occurs
        /// </summary>
        public Action<TransitionInfo<TState, TEventData>> Handler
        {
            get { return this.innerTransition.Handler; }
            set { this.innerTransition.Handler = value; }
        }

        /// <summary>
        /// Gets or sets a method which is invoked before this transition occurs, and can prevent the transition from occuring
        /// </summary>
        public Func<TransitionInfo<TState, TEventData>, bool> Guard
        {
            get { return this.innerTransition.Guard; }
            set { this.innerTransition.Guard = value; }
        }

        internal Transition(ITransitionInner<TState, Event<TEventData>, TransitionInfo<TState, TEventData>> innerTransition) 
        {
            this.innerTransition = innerTransition;
        }

        /// <summary>
        /// Sets a method which is invoked whenever this transition occurs
        /// </summary>
        /// <param name="handler">Method which is invoked whenever this transition occurs</param>
        /// <returns>This transition, for method chaining</returns>
        public Transition<TState, TEventData> WithHandler(Action<TransitionInfo<TState, TEventData>> handler)
        {
            this.Handler = handler;
            return this;
        }

        /// <summary>
        /// Sets a method which is invoked before this transition occurs, and can prevent the transition from occuring
        /// </summary>
        /// <param name="guard">method which is invoked before this transition occurs, and can prevent the transition from occuring</param>
        /// <returns>This transition, for method chaining</returns>
        public Transition<TState, TEventData> WithGuard(Func<TransitionInfo<TState, TEventData>, bool> guard)
        {
            this.Guard = guard;
            return this;
        }

        bool IInvokableTransition<TEventData>.TryInvoke(TEventData eventData)
        {
            var transitionInfo = new TransitionInfo<TState, TEventData>(this.From, this.To, this.Event, eventData, this.IsInnerTransition);
            return this.innerTransition.TryInvoke(transitionInfo);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object</returns>
        public override string ToString()
        {
            return String.Format("<Transition From={0} To={1} Event={2}{3}>", this.From.Name, this.To.Name, this.Event.Name, this.IsInnerTransition ? " IsInnerTransition" : "");
        }
    }

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
