using System;

namespace StateMechanic
{
    /// <summary>
    /// A serializer can turn the current state of a state machine into a string, or restore the state of a state machine from a string
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    public interface IStateMachineSerializer<TState> where TState : StateBase<TState>, new()
    {
        /// <summary>
        /// Yield the childmost state identified by the serialized string
        /// </summary>
        /// <param name="stateMachine">State machine to deserialize into</param>
        /// <param name="serialized">Serialized form of the state machine</param>
        /// <returns>The childmost state identified by the serialized string</returns>
        TState Deserialize(StateMachine<TState> stateMachine, string serialized);

        /// <summary>
        /// Record the current state of a state machine in a string
        /// </summary>
        /// <param name="stateMachine">State machine to serialize</param>
        /// <returns>Serialized state machine</returns>
        string Serialize(StateMachine<TState> stateMachine);
    }

    /// <summary>
    /// Exception indicating that serialization or deserialization has failed
    /// </summary>
    public class StateMachineSerializationException : Exception
    {

        /// <summary>
        /// Initialises a new instance of the <see cref="StateMachineSerializationException"/> class
        /// </summary>
        /// <param name="message">Exception message</param>
        public StateMachineSerializationException(string message)
            : base(message)
        {
        }
    }
}
