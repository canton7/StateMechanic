namespace StateMechanic
{
    /// <summary>
    /// Information about a transition
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    public interface ITransitionInfo<TState>
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
        /// The (untyped) event data, or null if there was none
        /// </summary>
        object EventData { get; }

        /// <summary>
        /// Gets the method used to fire the event
        /// </summary>
        EventFireMethod EventFireMethod { get; }
    }
}
