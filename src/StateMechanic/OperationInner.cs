using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StateMechanic
{
    internal class OperationInner<TState, TEvent>
        where TState : StateBase<TState>, new()
        where TEvent : IEvent
    {
        private readonly StateMachine<TState> stateMachine;

        public TEvent Event { get; }
        public IReadOnlyList<TState> OperationStates { get; }
        public IReadOnlyList<TState> SuccessStates { get; }

        public OperationInner(TEvent @event, IReadOnlyList<TState> operationStates, IReadOnlyList<TState> successStates)
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));
            if (operationStates == null)
                throw new ArgumentNullException(nameof(operationStates));
            if (successStates == null)
                throw new ArgumentNullException(nameof(successStates));

            if (operationStates.Count < 1)
                throw new ArgumentException("Must have at least one operationState", nameof(operationStates));
            if (successStates.Count < 1)
                throw new ArgumentException("Must have at least one successState", nameof(successStates));

            var sm = operationStates[0].ParentStateMachine.TopmostStateMachineInternal;
            if (!operationStates.Skip(1).Concat(successStates).All(x => x.ParentStateMachine.TopmostStateMachineInternal == sm))
                throw new ArgumentException("The topmost state machine for all operationStates and successStates must be the same");

            if (successStates.Any(x => operationStates.Contains(x)))
                throw new ArgumentException("operationStates and successStates may not contain any of the same states");

            this.Event = @event;
            this.OperationStates = operationStates;
            this.SuccessStates = successStates;
            this.stateMachine = sm;
        }

        public async Task<bool> InvokeAsync(Func<bool> eventFireAction, CancellationToken token)
        {
            var tcs = new TaskCompletionSource<object>();

            EventHandler<TransitionEventArgs<TState>> transitionHandler = (o, e) =>
            {
                if (!this.OperationStates.Contains(e.From))
                    return;

                if (this.SuccessStates.Contains(e.To))
                {
                    tcs.SetResult(null);
                }
                else if (!this.OperationStates.Contains(e.To))
                {
                    // Ignore transitions from operation states to other operation states
                    tcs.SetException(new OperationFailedException(this.OperationStates, this.SuccessStates, e.To));
                }
            };

            try
            {
                this.stateMachine.Transition += transitionHandler;

                using (token.Register(() => tcs.SetCanceled()))
                {
                    if (!(eventFireAction()))
                    {
                        return false;
                    }

                    await tcs.Task;
                    return true;
                }
            }
            finally
            {
                this.stateMachine.Transition -= transitionHandler;
            }
        }
    }
}
