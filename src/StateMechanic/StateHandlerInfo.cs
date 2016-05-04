namespace StateMechanic
{
    /// <summary>
    /// Information given to state entry/exit handlers
    /// </summary>
    /// <typeparam name="TState">Type of state being transitioned from and to</typeparam>
    public class StateHandlerInfo<TState>
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
        public IEvent Event { get; }

        /// <summary>
        /// Gets a value indicating whether this is an inner self transition, i.e. whether entry/exit handler are not executed
        /// </summary>
        public bool IsInnerTransition { get; }

        /// <summary>
        /// Gets the method used to fire the event
        /// </summary>
        public EventFireMethod EventFireMethod { get; }

        internal StateHandlerInfo(TState from, TState to, IEvent @event, bool isInnerTransition, EventFireMethod eventFireMethod)
        {
            this.From = from;
            this.To = to;
            this.Event = @event;
            this.IsInnerTransition = isInnerTransition;
            this.EventFireMethod = eventFireMethod;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object.</returns>
        [ExcludeFromCoverage]
        public override string ToString()
        {
            return $"<StateHandlerInfo From={this.From} To={this.To} Event={this.Event} IsInnerTransition={this.IsInnerTransition} EventFireMethod={this.EventFireMethod}>";
        }
    }
}
