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
        public TState From { get; private set; }

        /// <summary>
        /// Gets the state this transition is to
        /// </summary>
        public TState To { get; private set; }

        /// <summary>
        /// Gets the event which triggered this transition
        /// </summary>
        public IEvent Event { get; private set; }

        internal StateHandlerInfo(TState from, TState to, IEvent @event)
        {
            this.From = from;
            this.To = to;
            this.Event = @event;
        }
    }
}
