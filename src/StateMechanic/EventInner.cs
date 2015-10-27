using System;
using System.Collections.Generic;

namespace StateMechanic
{
    internal class EventInner<TEvent, TTransitionData>
    {
        private readonly Dictionary<IState, List<IInvokableTransition<TTransitionData>>> transitions = new Dictionary<IState, List<IInvokableTransition<TTransitionData>>>();

        public readonly string Name;
        public readonly IEventDelegate parentStateMachine;

        public EventInner(string name, IEventDelegate parentStateMachine)
        {
            this.Name = name;
            this.parentStateMachine = parentStateMachine;
        }

        public void AddTransition(IState state, IInvokableTransition<TTransitionData> transition)
        {
            List<IInvokableTransition<TTransitionData>> transitions;
            if (!this.transitions.TryGetValue(state, out transitions))
            {
                transitions = new List<IInvokableTransition<TTransitionData>>();
                this.transitions.Add(state, transitions);
            }

            transitions.Add(transition);
        }

        public bool Fire(TTransitionData transitionData, IEvent parentEvent, EventFireMethod eventFireMethod)
        {
            return this.parentStateMachine.RequestEventFireFromEvent(parentEvent, state =>
            {
                List<IInvokableTransition<TTransitionData>> transitions;
                if (!this.transitions.TryGetValue(state, out transitions))
                    return false;

                // Keep trying until one works (i.e. has a guard that lets it execute)
                bool anyFound = false;
                foreach (var transition in transitions)
                {
                    if (transition.TryInvoke(transitionData))
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
