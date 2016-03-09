
namespace StateMechanic
{
    /// <summary>
    /// A state machine, using <see cref="State"/> as its state
    /// </summary>
    public sealed class StateMachine : StateMachine<State>
    {
        /// <summary>
        /// Instantiates a new instance of the <see cref="StateMachine"/> class, with the given name
        /// </summary>
        /// <param name="name">Name of this state machine</param>
        public StateMachine(string name)
            : base(name)
        {
        }
    }
}
