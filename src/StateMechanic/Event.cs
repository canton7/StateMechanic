using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public bool Fire(Func<TTransition, bool> transitionInvoker, IEvent parentEvent, bool throwIfNotFound)
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
            }, throwIfNotFound);
        }
    }

    /// <summary>
    /// An event, which can be fired to trigger a transition from one state to antoher
    /// </summary>
    public class Event : IEvent
    {
        private readonly EventInner<Event, IInvokableTransition> innerEvent;

        /// <summary>
        /// Gets the name assigned to this event
        /// </summary>
        public string Name { get { return this.innerEvent.Name; } }

        /// <summary>
        /// Gets the state machine associated with this event. This event can be used to trigger transitions on its parent state machine, or any of its child state machines
        /// </summary>
        public IStateMachine ParentStateMachine { get { return this.innerEvent.parentStateMachine; } }

        internal Event(string name, IEventDelegate eventDelegate)
        {
            this.innerEvent = new EventInner<Event, IInvokableTransition>(name, eventDelegate);
        }

        internal void AddTransition(IState state, IInvokableTransition transition)
        {
            this.innerEvent.AddTransition(state, transition);
        }

        /// <summary>
        /// Attempt to fire this event, returning false if a transition on this event could not be found on the parent state machine's current state
        /// </summary>
        /// <remarks>
        /// No exception will be thrown if no transition on this event could not be found on the parent state machine's current state
        /// 
        /// NOTE! If fired from with a transition handler or entry/exit handler, this method will always return true.
        /// If the parent state machine has a <see cref="IStateMachineSynchronizer"/>, then the return value of this method may not correctly indicate whether the event was successfully fired
        /// </remarks>
        /// <returns>True if the event could not be fired.</returns>
        public bool TryFire()
        {
            return this.innerEvent.Fire(transition => transition.TryInvoke(), this, false);
        }

        public void Fire()
        {
            this.innerEvent.Fire(transition => transition.TryInvoke(), this, true);
        }

        public override string ToString()
        {
            return String.Format("<Event Name={0}>", this.Name);
        }
    }

    /// <summary>
    /// An event, which can be fired with some event data to trigger a transition from one state to antoher
    /// </summary>
    /// <typeparam name="TEventData">Type of event data which will be provided when the event is fired</typeparam>
    public class Event<TEventData> : IEvent
    {
        private readonly EventInner<Event<TEventData>, IInvokableTransition<TEventData>> innerEvent;

        /// <summary>
        /// Gets the name assigned to this event
        /// </summary>
        public string Name { get { return this.innerEvent.Name; } }

        /// <summary>
        /// Gets the state machine associated with this event. This event can be used to trigger transitions on its parent state machine, or any of its child state machines
        /// </summary>
        public IStateMachine ParentStateMachine { get { return this.innerEvent.parentStateMachine; } }

        internal Event(string name, IEventDelegate eventDelegate)
        {
            this.innerEvent = new EventInner<Event<TEventData>, IInvokableTransition<TEventData>>(name, eventDelegate);
        }

        internal void AddTransition(IState state, IInvokableTransition<TEventData> transition)
        {
            this.innerEvent.AddTransition(state, transition);
        }

        /// <summary>
        /// Attempt to fire this event, returning false if a transition on this event could not be found on the parent state machine's current state
        /// </summary>
        /// <remarks>
        /// No exception will be thrown if no transition on this event could not be found on the parent state machine's current state
        /// 
        /// NOTE! If fired from with a transition handler or entry/exit handler, this method will always return true.
        /// If the parent state machine has a <see cref="IStateMachineSynchronizer"/>, then the return value of this method may not correctly indicate whether the event was successfully fired
        /// </remarks>
        /// <param name="eventData">Event data to associate with this event</param>
        /// <returns>True if the event could not be fired.</returns>
        public bool TryFire(TEventData eventData)
        {
            return this.innerEvent.Fire(transition => transition.TryInvoke(eventData), this, false);
        }

        public void Fire(TEventData eventData)
        {
            this.innerEvent.Fire(transition => transition.TryInvoke(eventData), this, true);
        }

        void IEvent.Fire()
        {
            this.Fire(default(TEventData));
        }

        bool IEvent.TryFire()
        {
            return this.TryFire(default(TEventData));
        }

        public override string ToString()
        {
            return String.Format("<Event Name={0}>", this.Name);
        }
    }
}
