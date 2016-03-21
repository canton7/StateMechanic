namespace StateMechanic
{
    /// <summary>
    /// A group, which contains many states, and can have its own entry and exit handlers (specialsed to State state types)
    /// </summary>
    public class StateGroup : StateGroup<State>
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="StateGroup{TState}"/> class
        /// </summary>
        /// <param name="name">Name of this state group</param>
        public StateGroup(string name = null)
            : base(name)
        {
        }
    }
}
