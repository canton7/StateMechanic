using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace StateMechanic
{
    /// <summary>
    /// An operation is a collection of a state which represents an operation which takes
    /// time to complete, and event which can trigger a transition to that state, and a concept
    /// a state transitioned to when the operation completes, or when it fails.
    /// </summary>
    /// <typeparam name="TState">Type of state</typeparam>
    public class Operation<TState> where TState : StateBase<TState>, new()
    {
        private readonly OperationInner<TState, Event> operationInner;

        /// <summary>
        /// Event which can trigger a transition to the <see cref="OperationState"/>
        /// </summary>
        public Event Event => this.operationInner.Event;

        /// <summary>
        /// State which represents the operation being executed
        /// </summary>
        public TState OperationState => this.operationInner.OperationState;

        /// <summary>
        /// A collection of states which are transitioned to from <see cref="OperationState"/>
        /// when the operation completes successfully
        /// </summary>
        public IReadOnlyList<TState> SuccessStates => this.operationInner.SuccessStates;

        /// <summary>
        /// Instantiates a new instance of the <see cref="Operation{TState}"/> class
        /// </summary>
        /// <param name="event">Event which can trigger a transition to the operationState</param>
        /// <param name="operationState">State which represents the in-progress operation</param>
        /// <param name="successStates">States which can be transitioned to from the operationState which indicate a successful operation</param>
        public Operation(Event @event, TState operationState, params TState[] successStates)
        {
            this.operationInner = new OperationInner<TState, Event>(@event, operationState, new ReadOnlyCollection<TState>(successStates));
        }

        /// <summary>
        /// Attempt to call <see cref="Event.TryFire"/>, returning false straight away if it failed,
        /// or a Task which completes when a transition from the <see cref="OperationState"/> occurs
        /// otherwise.
        /// </summary>
        /// <remarks>It is NOT currently safe to call this from a transition handler</remarks>
        /// <param name="cancellationToken">Optional token which can cancel the operation</param>
        /// <returns>Task which completes if the event couldn't be fired, or when the operation completes</returns>
        public Task<bool> TryFireAsync(CancellationToken? cancellationToken = null)
        {
            return this.operationInner.InvokeAsync(() => this.Event.TryFire(), cancellationToken ?? CancellationToken.None);
        }

        /// <summary>
        /// Attempt to call <see cref="Event.TryFire"/>, returning false straight away if it failed,
        /// or a Task which completes when a transition from the <see cref="OperationState"/> occurs
        /// otherwise.
        /// </summary>
        /// <remarks>It is NOT currently safe to call this from a transition handler</remarks>
        /// <param name="timeout">Timeout after which to timeout the operation</param>
        /// <returns>Task which completes if the event couldn't be fired, or when the operation completes</returns>
        public Task<bool> TryFireAsync(TimeSpan timeout)
        {
            var cts = new CancellationTokenSource(timeout);
            return this.TryFireAsync(cts.Token);
        }

        /// <summary>
        /// Attempt to call <see cref="Event.Fire"/>, returning a Task containing the <see cref="TransitionFailedException"/>,
        /// or a Task which completes when a transition from the <see cref="OperationState"/> occurs
        /// otherwise.
        /// </summary>
        /// <remarks>It is NOT currently safe to call this from a transition handler</remarks>
        /// <param name="cancellationToken">Optional token which can cancel the operation</param>
        /// <returns>Task which completes if the event couldn't be fired, or when the operation completes</returns>
        public Task FireAsync(CancellationToken? cancellationToken = null)
        {
            return this.operationInner.InvokeAsync(() => { this.Event.Fire(); return true; }, cancellationToken ?? CancellationToken.None);
        }

        /// <summary>
        /// Attempt to call <see cref="Event.Fire"/>, returning a Task containing the <see cref="TransitionFailedException"/>,
        /// or a Task which completes when a transition from the <see cref="OperationState"/> occurs
        /// otherwise.
        /// </summary>
        /// <remarks>It is NOT currently safe to call this from a transition handler</remarks>
        /// <param name="timeout">Timeout after which to timeout the operation</param>
        /// <returns>Task which completes if the event couldn't be fired, or when the operation completes</returns>
        public Task FireAsync(TimeSpan timeout)
        {
            var cts = new CancellationTokenSource(timeout);
            return this.FireAsync(cts.Token);
        }
    }
}
