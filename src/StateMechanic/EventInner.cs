using System;
using System.Collections.Generic;

namespace StateMechanic
{
    internal class EventInner<TEvent, TTransitionData>
    {
        private readonly Dictionary<IState, List<IInvokableTransition<TTransitionData>>> transitions = new Dictionary<IState, List<IInvokableTransition<TTransitionData>>>();

        public readonly string Name;
        private readonly IEventInternal parentEvent;
        public readonly IEventDelegate parentStateMachine;

        public EventInner(string name, IEventInternal parentEvent, IEventDelegate parentStateMachine)
        {
            this.Name = name;
            this.parentEvent = parentEvent;
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

        public bool Fire(TTransitionData transitionData, EventFireMethod eventFireMethod)
        {
            var eventFireData = new EventFireData(this.parentEvent, eventFireMethod, transitionData);

            return this.parentStateMachine.RequestEventFireFromEvent(eventFireData);
            //parentEvent, state =>
            //{
            //    List<IInvokableTransition<TTransitionData>> transitions;
            //    if (!this.transitions.TryGetValue(state, out transitions))
            //        return false;

            //    // Keep trying until one works (i.e. has a guard that lets it execute)
            //    bool anyFound = false;
            //    foreach (var transition in transitions)
            //    {
            //        if (transition.TryInvoke(transitionData))
            //        {
            //            anyFound = true;
            //            break;
            //        }
            //    }

            //    return anyFound;
            //}, eventFireMethod);
        }

        // This is invoked by the state machine, after we call this.parentStateMachine.RequestEventFireFromEvent(eventFireData)
        public bool FireEventFromStateMachine(IState currentState, object eventData)
        {
            List<IInvokableTransition<TTransitionData>> transitions;
            if (!this.transitions.TryGetValue(currentState, out transitions))
                return false;

            // Keep trying until one works (i.e. has a guard that lets it execute)
            bool anyFound = false;
            foreach (var transition in transitions)
            {
                // This ugly cast is the consequence of trying to avoid copious delegate creation when firing events
                // 99% of the time it's going to be a reference cast, and therefore cheap. The other time we'll be doing
                // a struct unbox, but we'll still be saving creating a delegate object and generate delegate class, so we're still
                // winning.
                if (transition.TryInvoke((TTransitionData)eventData))
                {
                    anyFound = true;
                    break;
                }
            }

            return anyFound;
        }
    }
}
