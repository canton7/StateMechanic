using System.Collections.Generic;
using System.Linq;

namespace StateMechanic
{
    internal struct ForcedTransitionInvoker<TState> : ITransitionInvoker<TState>
        where TState : StateBase<TState>, new()
    {
        private readonly TState toState;
        private readonly ITransitionDelegate<TState> transitionDelegate;

        public EventFireMethod EventFireMethod { get; }
        public IEvent Event { get; }
        public object EventData { get; }

        public ForcedTransitionInvoker(TState toState, IEvent @event, object eventData, ITransitionDelegate<TState> transitionDelegate)
        {
            this.toState = toState;
            // This is never actually references, but needs to be part of ITransitionInvoker
            this.EventFireMethod = EventFireMethod.Fire;
            this.Event = @event;
            this.EventData = eventData;
            this.transitionDelegate = transitionDelegate;
        }

        public bool TryInvoke(TState sourceState)
        {
            // We support transitioning between states on different state machines. To do this, we have to find the lowest
            // common parent - our first transition is on that. Then we have to "walk" down the ancestry of parents of the 'to'
            // state.

            var states = EnumerateParentStates(this.toState).Reverse();
            foreach (var state in states)
            {
                if (state.ParentStateMachine.CurrentState != state)
                {
                    var transitionInfo = new ForcedTransitionInfo<TState>(state.ParentStateMachine.CurrentState, state, this.Event, this.EventData, this.EventFireMethod);
                    this.transitionDelegate.CoordinateTransition(transitionInfo, null);
                }
            }
            return true;
        }

        private static IEnumerable<TState> EnumerateParentStates(TState startingState)
        {
            for (TState state = startingState; state != null; state = state.ParentStateMachine.ParentState)
            {
                yield return state;
            }
        }
    }
}
