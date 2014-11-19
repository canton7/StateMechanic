using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    // Given an IState, returns an action to invoke the transition handler on it, or null if it doesn't exist
    internal delegate void EventInvocation(Func<IState, TransitionInvoker> transitionInvocation);

    internal class EventInner<TEvent, TTransition> where TTransition : ITransitionGuard
    {
        public string Name { get; private set; }

        private readonly EventInvocation invocation;
        private readonly Dictionary<IState, TTransition> transitions = new Dictionary<IState, TTransition>();

        public EventInner(string name, EventInvocation invocation)
        {
            this.Name = name;
            this.invocation = invocation;
        }

        public void AddTransition(IState state, TTransition transitionInvocation)
        {
            this.transitions.Add(state, transitionInvocation);
        }

        public void Fire(Action<TTransition> action)
        {
            this.invocation(state =>
            {
                TTransition transition;
                if (!this.transitions.TryGetValue(state, out transition))
                    return null;

                return new TransitionInvoker(transition.CanInvoke, () => action(transition));
            });
        }
    }

    public class Event<TEventData> : IEvent
    {
        private readonly EventInner<Event<TEventData>, ITransition<TEventData>> innerEvent;

        public string Name { get { return this.innerEvent.Name; } }

        internal Event(string name, EventInvocation invocation)
        {
            this.innerEvent = new EventInner<Event<TEventData>, ITransition<TEventData>>(name, invocation);
        }

        internal void AddTransition(IState state, ITransition<TEventData> transition)
        {
            this.innerEvent.AddTransition(state, transition);
        }

        public void Fire(TEventData eventData)
        {
            this.innerEvent.Fire(transition => transition.Invoke(eventData));
        }
    }

    public class Event : IEvent
    {
        private readonly EventInner<Event, ITransition> innerEvent;

        public string Name { get { return this.innerEvent.Name; } }

        internal Event(string name, EventInvocation invocation)
        {
            this.innerEvent = new EventInner<Event, ITransition>(name, invocation);
        }

        internal void AddTransition(IState state, ITransition transition)
        {
            this.innerEvent.AddTransition(state, transition);
        }

        public void Fire()
        {
            this.innerEvent.Fire(transition => transition.Invoke());
        }
    }
}
