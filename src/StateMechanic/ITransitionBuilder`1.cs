namespace StateMechanic
{
    /// <summary>
    /// The result of calling <see cref="State.TransitionOn(Event)"/> or <see cref="State{TStateData}.TransitionOn(Event)"/>, represents a builder creating a transition
    /// </summary>
    /// <typeparam name="TState">Type of state this transition will be from</typeparam>
    public interface ITransitionBuilder<TState> where TState : class, IState
    {
        /// <summary>
        /// Set the state this transition will transition to
        /// </summary>
        /// <param name="state">State this transition will transition to</param>
        /// <returns>The created transition, to which handlers can be added</returns>
        Transition<TState> To(TState state);
    }
}
