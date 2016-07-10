using System;
using System.Collections.Generic;
using System.Linq;

namespace StateMechanic
{
    internal class EventInner<TEvent, TTransition>
        where TTransition : ITransition
    {
        private readonly Dictionary<IState, List<TTransition>> transitions = new Dictionary<IState, List<TTransition>>();

        private IEventDelegate parentStateMachine;

        public void AddTransition(IState state, TTransition invokableTransition)
        {
            List<TTransition> transitions;
            if (!this.transitions.TryGetValue(state, out transitions))
            {
                transitions = new List<TTransition>();
                this.transitions.Add(state, transitions);
            }

            var firstThatWillAlwaysOccur = transitions.FirstOrDefault(x => x.WillAlwaysOccur);
            if (firstThatWillAlwaysOccur != null)
                throw new ArgumentException($"{invokableTransition} will never occur: {firstThatWillAlwaysOccur} was added first, and will always happen");

            transitions.Add(invokableTransition);
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
            if (this.parentStateMachine != null && this.parentStateMachine != parentStateMachine)
                throw new InvalidEventTransitionException(state, @event);

            this.parentStateMachine = parentStateMachine;
        }

        public bool RequestEventFireFromEvent(Event @event, EventFireMethod eventFireMethod)
        {
            if (this.parentStateMachine == null)
                throw new InvalidEventSetupException(@event);

            return this.parentStateMachine.RequestEventFireFromEvent(@event, eventFireMethod);
        }

        public bool RequestEventFireFromEvent<TEventData>(Event<TEventData> @event, TEventData eventData, EventFireMethod eventFireMethod)
        {
            if (this.parentStateMachine == null)
            {
                if (eventFireMethod == EventFireMethod.Fire)
                    throw new InvalidEventSetupException(@event);
                else
                    return false;
            }

            return this.parentStateMachine.RequestEventFireFromEvent(@event, eventData, eventFireMethod);
        }
    }
}
