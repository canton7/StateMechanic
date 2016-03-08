using System;
using System.Collections.Generic;

namespace StateMechanic
{
    public interface IStateMachineSerializer<TState> where TState : StateBase<TState>, new()
    {
        IEnumerable<TState> Deserialize(ChildStateMachine<TState> stateMachine, string serialized);

        string Serialize(ChildStateMachine<TState> stateMachine);
    }

    public class StateMachineSerializationException : Exception
    {
        public StateMachineSerializationException(string message)
            : base(message)
        {
        }
    }
}
