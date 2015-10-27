namespace StateMechanic
{
    /// <summary>
    /// An event, which can be fired to trigger a transition from one state to antoher
    /// </summary>
    public class Event : IEvent
    {
        private readonly EventInner<Event, IInvokableTransition> innerEvent;

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
            this.innerEvent = new EventInner<Event, IInvokableTransition>(name, parentStateMachine);
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
        /// NOTE! If fired from within a transition handler or entry/exit handler, this method will always return true.
        /// If the parent state machine has a <see cref="IStateMachineSynchronizer"/>, then the return value of this method may not correctly indicate whether the event was successfully fired
        /// </remarks>
        /// <returns>True if the event could not be fired.</returns>
        public bool TryFire()
        {
            return this.innerEvent.Fire(transition => transition.TryInvoke(), this, EventFireMethod.TryFire);
        }

        /// <summary>
        /// Attempt to fire this event, throwing a <see cref="TransitionNotFoundException"/> if a transition on this event could not be found on the parent state machine's current state
        /// </summary>
        /// <remarks>
        /// NOTE! If fired from within a transition handler or entry/exit hander, this method will never throw an exception.
        /// However, the call to <see cref="Fire()"/> or <see cref="TryFire()"/> which originally triggered the outermost
        /// transition may result in an exception being thrown.
        /// 
        /// If the parent state machine has a <see cref="IStateMachineSynchronizer"/>, then exception-throwing behaviour will be determined by that synchronizer.
        /// </remarks>
        public void Fire()
        {
            this.innerEvent.Fire(transition => transition.TryInvoke(), this, EventFireMethod.Fire);
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
