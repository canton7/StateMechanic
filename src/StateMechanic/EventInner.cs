using System;
using System.Collections.Generic;

namespace StateMechanic
{
    internal class EventInner<TEvent, TTransition>
    {
        private readonly Dictionary<IState, List<TTransition>> transitions = new Dictionary<IState, List<TTransition>>();

        public readonly string Name;
        public readonly IEventDelegate parentStateMachine;

        public EventInner(string name, IEventDelegate parentStateMachine)
        {
            this.Name = name;
            this.parentStateMachine = parentStateMachine;
        }

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

        public bool Fire(Func<TTransition, bool> transitionInvoker, IEvent parentEvent, EventFireMethod eventFireMethod)
        {
            return this.parentStateMachine.RequestEventFireFromEvent(parentEvent, state =>
            {
                List<TTransition> transitions;
                if (!this.transitions.TryGetValue(state, out transitions))
                    return false;

                // Keep trying until one works (i.e. has a guard that lets it execute)
                bool anyFound = false;
                foreach (var transition in transitions)
                {
                    if (transitionInvoker(transition))
                    {
                        anyFound = true;
                        break;
                    }
                }

                return anyFound;
            }, eventFireMethod);
        }
    }
}
