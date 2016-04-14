namespace StateMechanic
{
    /// <summary>
    /// Contains information on the currently-executing transition
    /// </summary>
    /// <typeparam name="TState">Type of state</typeparam>
    public class TransitionInfo<TState>
    {
        /// <summary>
        /// Gets the state this transition is from
        /// </summary>
        public TState From { get; }

        /// <summary>
        /// Gets the state this transition is to
        /// </summary>
        public TState To { get; }

        /// <summary>
        /// Gets the event which triggered this transition
        /// </summary>
        public Event Event { get; }

        /// <summary>
        /// Gets a value indicating whether this is an inner self transition, i.e. whether entry/exit handler are not executed
        /// </summary>
        public bool IsInnerTransition { get; }

        internal TransitionInfo(TState from, TState to, Event @event, bool isInnerTransition)
        {
            this.From = from;
            this.To = to;
            this.Event = @event;
            this.IsInnerTransition = isInnerTransition;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"<TransitionInfo From=${this.From} To={this.To} Event={this.Event} IsInnerTransition={this.IsInnerTransition}>";
        }
    }
}
