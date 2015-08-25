using System;

namespace StateMechanic
{
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
        public string Name => this.innerEvent.Name;

        /// <summary>
        /// Gets the state machine associated with this event. This event can be used to trigger transitions on its parent state machine, or any of its child state machines
        /// </summary>
        public IStateMachine ParentStateMachine => this.innerEvent.parentStateMachine;

        internal Event(string name, IEventDelegate parentStateMachine)
        {
            this.innerEvent = new EventInner<Event<TEventData>, IInvokableTransition<TEventData>>(name, parentStateMachine);
        }

        internal void AddTransition(IState state, IInvokableTransition<TEventData> transition)
        {
            this.innerEvent.AddTransition(state, transition);
        }

        /// <summary>
        /// Attempt to fire this event, returning false if a transition on this event could not be found on the parent state machine's current state
        /// </summary>
        /// <remarks>
        /// No exception will be thrown if no transition on this event could not be found on the parent state machine's current state.
        /// 
        /// NOTE! If fired from within a transition handler or entry/exit handler, this method will always return true.
        /// If the parent state machine has a <see cref="IStateMachineSynchronizer"/>, then the return value of this method may not correctly indicate whether the event was successfully fired
        /// </remarks>
        /// <param name="eventData">Event data to associate with this event</param>
        /// <returns>True if the event could not be fired.</returns>
        public bool TryFire(TEventData eventData)
        {
            return this.innerEvent.Fire(transition => transition.TryInvoke(eventData), this, EventFireMethod.TryFire);
        }

        /// <summary>
        /// Attempt to fire this event, throwing a <see cref="TransitionNotFoundException"/> if a transition on this event could not be found on the parent state machine's current state
        /// </summary>
        /// <remarks>
        /// NOTE! If fired from within a transition handler or entry/exit hander, this method will never throw an exception.
        /// However, the call to <see cref="Fire(TEventData)"/> or <see cref="TryFire(TEventData)"/> which originally triggered the outermost
        /// transition may result in an exception being thrown.
        /// 
        /// If the parent state machine has a <see cref="IStateMachineSynchronizer"/>, then exception-throwing behaviour will be determined by that synchronizer.
        /// </remarks>
        /// <param name="eventData">Event data to associate with this event</param>
        public void Fire(TEventData eventData)
        {
            this.innerEvent.Fire(transition => transition.TryInvoke(eventData), this, EventFireMethod.Fire);
        }

        void IEvent.Fire()
        {
            this.Fire(default(TEventData));
        }

        bool IEvent.TryFire()
        {
            return this.TryFire(default(TEventData));
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object</returns>
        public override string ToString()
        {
            return $"<Event Name={this.Name}>";
        }
    }
}
