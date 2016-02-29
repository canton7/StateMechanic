namespace StateMechanic
{
    /// <summary>
    /// A transition from one state to another, triggered by an event, where the destination state is determined by a user-supplied callback
    /// </summary>
    /// <typeparam name="TState">Type of state which this transition is between</typeparam>
    public interface IDynamicTransition<out TState> where TState : IState
    {
        /// <summary>
        /// Gets the state this transition is from
        /// </summary>
        TState From { get; }

        /// <summary>
        /// Gets the event which triggers this transition
        /// </summary>
        IEvent Event { get; }
    }
}
