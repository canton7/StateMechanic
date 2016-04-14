using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace StateMechanic
{
    internal class ForcedTransitionInvoker<TState> : ITransitionInvoker<TState> where TState : StateBase<TState>, new()
    {
        private readonly TState toState;
        private readonly ITransitionDelegate<TState> transitionDelegate;

        public EventFireMethod EventFireMethod { get; }
        public IEvent Event { get; }

        public ForcedTransitionInvoker(TState toState, IEvent @event, ITransitionDelegate<TState> transitionDelegate)
        {
            this.toState = toState;
            this.EventFireMethod = EventFireMethod.Fire;
            this.Event = @event;
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
                    this.transitionDelegate.CoordinateTransition<object>(state.ParentStateMachine.CurrentState, state, this.Event, false, null, null);
                }
            }

            // No transition data (no handler)
            //this.transitionDelegate.CoordinateTransition<object>(sourceState, this.toState, this.Event, false, null, null);
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
