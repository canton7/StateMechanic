using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    // Given an IState, returns an action to invoke the transition handler on it, or null if it doesn't exist
    internal delegate bool EventInvocation(Func<IState, Action> transitionInvocation);

    internal class EventInner<TEvent, TTransition>
    {
        private readonly Dictionary<IState, List<TTransition>> transitions = new Dictionary<IState, List<TTransition>>();

        public readonly string Name;
        public readonly IEventDelegate eventDelegate;

        public EventInner(string name, IEventDelegate eventDelegate)
        {
            this.Name = name;
            this.eventDelegate = eventDelegate;
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

        public bool Fire(Func<TTransition, bool> transitionInvoker, IEvent parentEvent, bool throwIfNotFound)
        {
            return this.eventDelegate.RequestEventFire(state =>
            {
                List<TTransition> transitions;
                if (!this.transitions.TryGetValue(state, out transitions))
                {
                    this.eventDelegate.NotifyTransitionNotFound(parentEvent);
                    if (throwIfNotFound)
                        throw new TransitionNotFoundException(state, parentEvent);
                    return false;
                }

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
            });
        }
    }

    public class Event<TEventData> : IEvent
    {
        private readonly EventInner<Event<TEventData>, IInvocableTransition<TEventData>> innerEvent;

        public string Name { get { return this.innerEvent.Name; } }
        public IStateMachine ParentStateMachine { get { return this.innerEvent.eventDelegate; } }

        internal Event(string name, IEventDelegate eventDelegate)
        {
            this.innerEvent = new EventInner<Event<TEventData>, IInvocableTransition<TEventData>>(name, eventDelegate);
        }

        internal void AddTransition(IState state, IInvocableTransition<TEventData> transition)
        {
            this.innerEvent.AddTransition(state, transition);
        }

        public bool Fire(TEventData eventData)
        {
            return this.innerEvent.Fire(transition => transition.TryInvoke(eventData), this, false);
        }

        public void EnsureFire(TEventData eventData)
        {
            this.innerEvent.Fire(transition => transition.TryInvoke(eventData), this, true);
        }
    }

    public class Event : IEvent
    {
        private readonly EventInner<Event, IInvocableTransition> innerEvent;

        public string Name { get { return this.innerEvent.Name; } }
        public IStateMachine ParentStateMachine { get { return this.innerEvent.eventDelegate; } }

        internal Event(string name, IEventDelegate eventDelegate)
        {
            this.innerEvent = new EventInner<Event, IInvocableTransition>(name, eventDelegate);
        }

        internal void AddTransition(IState state, IInvocableTransition transition)
        {
            this.innerEvent.AddTransition(state, transition);
        }

        public bool Fire()
        {
            return this.innerEvent.Fire(transition => transition.TryInvoke(), this, false);
        }

        public void EnsureFire()
        {
            this.innerEvent.Fire(transition => transition.TryInvoke(), this, true);
        }
    }
}
