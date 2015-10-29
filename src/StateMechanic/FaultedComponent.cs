namespace StateMechanic
{
    /// <summary>
    /// Indicates which component of the transition threw an exception
    /// </summary>
    public enum FaultedComponent
    {
        /// <summary>
        /// A state exit handler threw an excpetion
        /// </summary>
        ExitHandler,

        /// <summary>
        /// A StateGroup exit handler threw an exception
        /// </summary>
        GroupExitHandler,

        /// <summary>
        /// A transition handler threw an exception
        /// </summary>
        TransitionHandler,

        /// <summary>
        /// A StateGroup entry handler threw an exception
        /// </summary>
        GroupEntryHandler,

        /// <summary>
        /// A state entry handler threw an exception
        /// </summary>
        EntryHandler,
    }
}
