using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    // Given an IState, returns an action to invoke the transition handler on it, or null if it doesn't exist
    internal delegate bool EventInvocation(Func<IState, Action<TransitionInvocationState>> transitionInvocation);

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

        public bool TryFire(Action<TTransition, TransitionInvocationState> action)
        {
            var currentState = this.eventDelegate.CurrentState;

            TTransition transition;
            if (!this.transitions.TryGetValue(currentState, out transition) || !transition.CanInvoke())
                return false;

            this.eventDelegate.FireEvent(transitionInvocationState => action(transition, transitionInvocationState));
            return true;
        }
    }

    public class Event<TEventData> : IEvent
    {
        private readonly EventInner<Event<TEventData>, ITransition<TEventData>> innerEvent;

        public string Name { get { return this.innerEvent.Name; } }

        internal Event(string name, IEventDelegate eventDelegate)
        {
            this.innerEvent = new EventInner<Event<TEventData>, ITransition<TEventData>>(name, eventDelegate);
        }

        internal void AddTransition(IState state, ITransition<TEventData> transition)
        {
            this.innerEvent.AddTransition(state, transition);
        }

        public bool TryFire(TEventData eventData)
        {
            return this.innerEvent.TryFire((transition, state) => transition.Invoke(eventData, state));
        }

        public void Fire(TEventData eventData)
        {
            if (!this.TryFire(eventData))
                throw new TransitionNotFoundException(this.innerEvent.eventDelegate.CurrentState, this);
        }
    }

    public class Event : IEvent
    {
        private readonly EventInner<Event, ITransition> innerEvent;

        public string Name { get { return this.innerEvent.Name; } }

        internal Event(string name, IEventDelegate eventDelegate)
        {
            this.innerEvent = new EventInner<Event, ITransition>(name, eventDelegate);
        }

        internal void AddTransition(IState state, ITransition transition)
        {
            this.innerEvent.AddTransition(state, transition);
        }

        public bool TryFire()
        {
            return this.innerEvent.TryFire((transition, state) => transition.Invoke(state));
        }

        public void Fire()
        {
            if (!this.TryFire())
                throw new TransitionNotFoundException(this.innerEvent.eventDelegate.CurrentState, this);
        }
    }
}
