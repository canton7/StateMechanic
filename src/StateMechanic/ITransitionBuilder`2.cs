namespace StateMechanic
{
    /// <summary>
    /// The result of calling <see cref="StateBase{TState}.TransitionOn(Event)"/> or <see cref="StateBase{TState}.TransitionOn{TEventData}(Event{TEventData})"/>, represents a builder creating a transition
    /// </summary>
    /// <typeparam name="TState">Type of state this transition will be from</typeparam>
    /// <typeparam name="TEventData">Type of event data associted with this transition</typeparam>
    public interface ITransitionBuilder<TState, TEventData> where TState : class, IState
    {
        /// <summary>
        /// Set the state this transition will transition to
        /// </summary>
        /// <param name="state">State this transition will transition to</param>
        /// <returns>The created transition, to which handlers can be added</returns>
        Transition<TState, TEventData> To(TState state);
    }
}
