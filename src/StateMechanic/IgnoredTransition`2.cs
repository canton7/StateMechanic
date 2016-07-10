using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    internal class IgnoredTransition<TState, TEventData> : IInvokableTransition<TEventData>
        where TState : StateBase<TState>, new()
    {
        private readonly ITransitionDelegate<TState> transitionDelegate;
        private readonly TState fromState;
        private readonly IEvent @event;

        public IgnoredTransition(TState fromState, IEvent @event, ITransitionDelegate<TState> transitionDelegate)
        {
            this.transitionDelegate = transitionDelegate;
            this.fromState = fromState;
            this.@event = @event;
        }

        public bool TryInvoke(TEventData eventData, EventFireMethod eventFireMethod)
        {
            this.transitionDelegate.IgnoreTransition(this.fromState, this.@event, eventFireMethod);
            return true;
        }

        public override string ToString()
        {
            return $"<IgnoredTransition Event={this.@event}>";
        }
    }
}
