using System;
using System.Collections.Generic;

namespace StateMechanic
{
    /// <summary>
    /// Exception thrown when a <see cref="Operation{TState}"/> or <see cref="Operation{TState, TEventData}"/> did not complete successfully
    /// (that is, one of the <see cref="Operation{TState}.OperationStates"/> transitioned to a state which wasn't in <see cref="Operation{TState}.SuccessStates"/>).
    /// </summary>
    public class OperationFailedException : Exception
    {
        /// <summary>
        /// The states representing the operation
        /// </summary>
        public IReadOnlyList<IState> OperationStates { get; }

        /// <summary>
        /// The possible success states
        /// </summary>
        public IReadOnlyList<IState> SuccessStates { get; }

        /// <summary>
        /// The actual state whichw as transitioned to
        /// </summary>
        public IState ActualState { get; }

        internal OperationFailedException(IReadOnlyList<IState> operationStates, IReadOnlyList<IState> successStates, IState actualState)
            : base($"Operation failed. Expected a transition from one of [{String.Join(", ", operationStates)}] to one of [{String.Join(", ", successStates)}], but actually reached {actualState}")
        {
            this.OperationStates = operationStates;
            this.SuccessStates = successStates;
            this.ActualState = actualState;
        }
    }
}
