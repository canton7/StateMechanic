using System.Collections.Generic;
using System.Linq;

namespace StateMechanic
{
    internal class EventInner<TEvent, TTransition>
    {
        private readonly Dictionary<IState, List<TTransition>> transitions = new Dictionary<IState, List<TTransition>>();

        public IEventDelegate ParentStateMachine { get; set; }

        public void AddTransition(IState state, TTransition transitionInvocation)
        {
            List<TTransition> transitions;
            if (!this.transitions.TryGetValue(state, out transitions))
            {
                transitions = new List<TTransition>();
                this.transitions.Add(state, transitions);
            }

            transitions.Add(transitionInvocation);
        }

        public IEnumerable<TTransition> GetTransitionsForState(IState state)
        {
            List<TTransition> transitions;
            if (!this.transitions.TryGetValue(state, out transitions))
                return Enumerable.Empty<TTransition>();

            return transitions;
        }

        public void SetParentStateMachine(IEventDelegate parentStateMachine, IState state, IEvent @event)
        {
            var topStateMachine = parentStateMachine.TopmostStateMachine;
            if (this.ParentStateMachine != null && this.ParentStateMachine != topStateMachine)
                throw new InvalidEventTransitionException(state, @event);

            this.ParentStateMachine = topStateMachine;
        }
    }
}
