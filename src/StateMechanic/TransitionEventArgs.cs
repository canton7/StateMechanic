using System;

namespace StateMechanic
{
    /// <summary>
    /// Event args containing information on a transition which executed
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    public class TransitionEventArgs<TState> : EventArgs
    {
        /// <summary>
        /// Gets the state this transition wsas from
        /// </summary>
        public TState From { get; }

        /// <summary>
        /// Gets the state this transition was to
        /// </summary>
        public TState To { get; }

        /// <summary>
        /// Gets the event which triggered this transition
        /// </summary>
        public IEvent Event { get; }

        /// <summary>
        /// Gets the state machine on which the transition occurred
        /// </summary>
        public IStateMachine StateMachine { get; }

        /// <summary>
        /// Gets a value indicating whether this is an inner self transition, i.e. whether entry/exit handler are not executed
        /// </summary>
        public bool IsInnerTransition { get; }

        internal TransitionEventArgs(TState from, TState to, IEvent @event, IStateMachine stateMachine, bool isInnerTransition)
        {
            this.From = from;
            this.To = to;
            this.Event = @event;
            this.StateMachine = stateMachine;
            this.IsInnerTransition = isInnerTransition;
        }
    }
}
