using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    // Given an IState, returns an action to invoke the transition handler on it, or null if it doesn't exist
    internal delegate bool EventInvocation(Func<IState, Action> transitionInvocation);

    internal class EventInner<TEvent, TTransition> where TTransition : ITransitionGuard
    {
        private readonly Dictionary<IState, TTransition> transitions = new Dictionary<IState, TTransition>();

        public readonly string Name;
        public readonly IEventDelegate eventDelegate;

        public EventInner(string name, IEventDelegate eventDelegate)
        {
            this.Name = name;
            this.eventDelegate = eventDelegate;
        }

        public void AddTransition(IState state, TTransition transitionInvocation)
        {
            this.transitions.Add(state, transitionInvocation);
        }

        public bool Fire(Action<TTransition> action)
        {
            return this.eventDelegate.RequestEventFire(state =>
            {
                TTransition transition;
                if (!this.transitions.TryGetValue(state, out transition) || !transition.CanInvoke())
                    return false;

                action(transition);
                return true;
            });
        }
    }

    public class Event<TEventData> : IEvent
    {
        private readonly EventInner<Event<TEventData>, IInvocableTransition<TEventData>> innerEvent;

        public string Name { get { return this.innerEvent.Name; } }

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
            return this.innerEvent.Fire((transition) => transition.Invoke(eventData));
        }

        public void EnsureFire(TEventData eventData)
        {
            if (!this.Fire(eventData))
                throw new TransitionNotFoundException(this.innerEvent.eventDelegate.CurrentState, this);
        }
    }

    public class Event : IEvent
    {
        private readonly EventInner<Event, IInvocableTransition> innerEvent;

        public string Name { get { return this.innerEvent.Name; } }

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
            return this.innerEvent.Fire((transition) => transition.Invoke());
        }

        public void EnsureFire()
        {
            if (!this.Fire())
                throw new TransitionNotFoundException(this.innerEvent.eventDelegate.CurrentState, this);
        }
    }
}
