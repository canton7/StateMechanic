using System;
using System.Collections.Generic;

namespace StateMechanic
{
    /// <summary>
    /// Exception thrown when a <see cref="Operation{TState}"/> or <see cref="Operation{TState, TEventData}"/> did not complete successfully
    /// (that is, the <see cref="Operation{TState}.OperationState"/> transitioned to a state which wasn't in <see cref="Operation{TState}.SuccessStates"/>).
    /// </summary>
    public class OperationFailedException : Exception
    {
        /// <summary>
        /// The state representing the operation
        /// </summary>
        public IState OperationState { get; }

        /// <summary>
        /// The possible success states
        /// </summary>
        public IReadOnlyList<IState> SuccessStates { get; }

        /// <summary>
        /// The actual state whichw as transitioned to
        /// </summary>
        public IState ActualState { get; }

        internal OperationFailedException(IState operationState, IReadOnlyList<IState> successStates, IState actualState)
            : base($"Operation failed. Expected a transition from {operationState} to one of [{String.Join(", ", successStates)}, but actually reached {actualState}")
        {
            this.OperationState = operationState;
            this.SuccessStates = successStates;
            this.ActualState = actualState;
        }
    }
}
