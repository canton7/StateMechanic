namespace StateMechanic
{
    /// <summary>
    /// Mechanism by which an event was fired
    /// </summary>
    public enum EventFireMethod
    {
        /// <summary>
        /// Event was fired using Fire()
        /// </summary>
        Fire,

        /// <summary>
        /// Event was fired using TryFire
        /// </summary>
        TryFire,
    }
}
