using System;
using System.Collections.Generic;

namespace StateMechanic
{
    /// <summary>
    /// A serializer can turn the current state of a state machine into a string, or restore the state of a state machine from a string
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    public interface IStateMachineSerializer<TState> where TState : StateBase<TState>, new()
    {
        /// <summary>
        /// Restore the state of a state machine from a string
        /// </summary>
        /// <remarks>
        /// This method operates by yielding the states for successive child state machines in a state machine hierarchy.
        /// For a non-hierarchical state machine, this method will yield one state: the state of the state machine. 
        /// For a hierarchical state machine, this method will first yield the state of the topmost state machine. If that state has a child
        /// state machine, this method will then yield the state for that child state machine, and so on.
        /// </remarks>
        /// <param name="stateMachine">State machine to deserialize into</param>
        /// <param name="serialized">Serialized form of the state machine</param>
        /// <returns>Successive current states for each state machine in a hierarchy, see the remarks</returns>
        IEnumerable<TState> Deserialize(StateMachine<TState> stateMachine, string serialized);

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
