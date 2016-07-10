namespace StateMechanic
{
    /// <summary>
    /// Information given to state entry/exit handlers
    /// </summary>
    /// <typeparam name="TState">Type of state being transitioned from and to</typeparam>
    public interface IStateHandlerInfo<TState>
    {
        /// <summary>
        /// Gets the state this transition is from
        /// </summary>
        TState From { get; }

        /// <summary>
        /// Gets the state this transition is to
        /// </summary>
        TState To { get; }

        /// <summary>
        /// Gets the event which triggered this transition
        /// </summary>
        IEvent Event { get; }

        /// <summary>
        /// Gets a value indicating whether this is an inner self transition, i.e. whether entry/exit handler are not executed
        /// </summary>
        bool IsInnerTransition { get; }

        /// <summary>
        /// Gets the method used to fire the event
        /// </summary>
        EventFireMethod EventFireMethod { get; }
    }
}
