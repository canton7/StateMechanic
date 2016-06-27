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
        public TEvent Event { get; }
        public TState OperationState { get; }
        public IReadOnlyList<TState> SuccessStates { get; }

        public OperationInner(TEvent @event, TState operationState, IReadOnlyList<TState> successStates)
        {
            this.Event = @event;
            this.OperationState = operationState;
            this.SuccessStates = successStates;
        }

        public async Task<bool> InvokeAsync(Func<bool> eventFireAction, CancellationToken token)
        {
            var sm = this.OperationState.ParentStateMachine.TopmostStateMachineInternal;

            var tcs = new TaskCompletionSource<object>();

            EventHandler<TransitionEventArgs<TState>> transitionHandler = (o, e) =>
            {
                if (e.From != this.OperationState)
                    return;

                if (this.SuccessStates.Contains(e.To))
                {
                    tcs.SetResult(null);
                }
                else
                {
                    tcs.SetException(new OperationFailedException(this.OperationState, this.SuccessStates, e.To));
                }
            };

            try
            {
                sm.Transition += transitionHandler;

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
                sm.Transition -= transitionHandler;
            }
        }
    }
}
