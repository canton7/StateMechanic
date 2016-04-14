namespace StateMechanic
{
    /// <summary>
    /// A transition from one state to another, triggered by an event
    /// </summary>
    /// <typeparam name="TState">Type of state which this transition is between</typeparam>
    public interface ITransition<out TState> where TState : IState
    {
        /// <summary>
        /// Gets the state this transition is from
        /// </summary>
        TState From { get; }

        /// <summary>
        /// Gets the state this transition to
        /// </summary>
        TState To { get; }

        /// <summary>
        /// Gets the event which triggers this transition
        /// </summary>
        IEvent Event { get; }

        /// <summary>
        /// Gets a value indicating whether this transition is dynamic - i.e. the To state is determined by a callback.
        /// The <see cref="To"/> property will be null if this property is true.
        /// </summary>
        bool IsDynamicTransition { get; }

        /// <summary>
        /// Gets a value indicating whether this transition is an inner transition, i.e. whether the <see cref="From"/> and <see cref="To"/> states are the same, and no exit/entry handles are invoked
        /// </summary>
        bool IsInnerTransition { get; }

        /// <summary>
        /// Gets a value indicating whether this transition has a guard
        /// </summary>
        bool HasGuard { get; }
    }
}
