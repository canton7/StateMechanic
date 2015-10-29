using System.Collections.Generic;
using System.Linq;

namespace StateMechanic
{
    internal class EventInner<TEvent, TTransition>
    {
        private readonly Dictionary<IState, List<TTransition>> transitions = new Dictionary<IState, List<TTransition>>();

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
    }
}
