using System;
using System.Collections.Generic;
using System.Linq;

namespace StateMechanic
{
    /// <summary>
    /// An event, which can be fired to trigger a transition from one state to antoher
    /// </summary>
    public class Event : IEvent
    {
        private readonly Dictionary<IState, OptimizingList<IInvokableTransition>> transitions = new Dictionary<IState, OptimizingList<IInvokableTransition>>();
        private IEventDelegate parentStateMachine;

        /// <summary>
        /// Gets the name assigned to this event
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initialises a new instance of the <see cref="Event"/> class
        /// </summary>
        /// <param name="name">Name assigned to the evnet</param>
        public Event(string name = null)
        {
            this.Name = name;
        }

        internal void AddTransition(IState state, IInvokableTransition transition, IEventDelegate parentStateMachine)
        {
            this.SetParentStateMachine(parentStateMachine, state);

            OptimizingList<IInvokableTransition> transitions;
            if (!this.transitions.TryGetValue(state, out transitions))
            {
                transitions = new OptimizingList<IInvokableTransition>();
            }

            var firstThatWillAlwaysOccur = transitions.FirstOrDefault(x => x.WillAlwaysOccur);
            if (firstThatWillAlwaysOccur != null)
                throw new ArgumentException($"{transition} will never occur from {state}: {firstThatWillAlwaysOccur} was added first, and will always happen");

            transitions.Add(transition);
            this.transitions[state] = transitions;
        }

        private void SetParentStateMachine(IEventDelegate parentStateMachine, IState state)
        {
            if (this.parentStateMachine != null && this.parentStateMachine != parentStateMachine)
                throw new InvalidEventTransitionException(state, this);

            this.parentStateMachine = parentStateMachine;
        }

        internal IEnumerable<IInvokableTransition> GetTransitionsFromState(IState state)
        {
            OptimizingList<IInvokableTransition> transitions;
            if (!this.transitions.TryGetValue(state, out transitions))
                return Enumerable.Empty<IInvokableTransition>();

            return transitions.GetEnumerable();
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
        /// <returns>True if the event could be fired.</returns>
        public bool TryFire()
        {
            return this.RequestEventFireFromEvent(EventFireMethod.TryFire);
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
            this.RequestEventFireFromEvent(EventFireMethod.Fire);
        }


        private bool RequestEventFireFromEvent(EventFireMethod eventFireMethod)
        {
            if (this.parentStateMachine == null)
            {
                throw new InvalidEventSetupException(this);
            }

            return this.parentStateMachine.RequestEventFireFromEvent(this, eventFireMethod);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object</returns>
        [ExcludeFromCoverage]
        public override string ToString()
        {
            return $"<Event Name={this.Name ?? "(unnamed)"}>";
        }
    }   

    /// <summary>
    /// An event, which can be fired with some event data to trigger a transition from one state to antoher
    /// </summary>
    /// <typeparam name="TEventData">Type of event data which will be provided when the event is fired</typeparam>
    public class Event<TEventData> : IEvent
    {
        private readonly Dictionary<IState, OptimizingList<IInvokableTransition<TEventData>>> transitions = new Dictionary<IState, OptimizingList<IInvokableTransition<TEventData>>>();
        private IEventDelegate parentStateMachine;

        /// <summary>
        /// Gets the name assigned to this event
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initialises a new instance of the <see cref="Event{TEventData}"/> class
        /// </summary>
        /// <param name="name">Name assigned to the evnet</param>
        public Event(string name = null)
        {
            this.Name = name;
        }

        internal void AddTransition(IState state, IInvokableTransition<TEventData> transition, IEventDelegate parentStateMachine)
        {
            this.SetParentStateMachine(parentStateMachine, state);

            OptimizingList<IInvokableTransition<TEventData>> transitions;
            if (!this.transitions.TryGetValue(state, out transitions))
            {
                transitions = new OptimizingList<IInvokableTransition<TEventData>>();
            }

            var firstThatWillAlwaysOccur = transitions.FirstOrDefault(x => x.WillAlwaysOccur);
            if (firstThatWillAlwaysOccur != null)
                throw new ArgumentException($"{transition} will never occur from {state}: {firstThatWillAlwaysOccur} was added first, and will always happen");

            transitions.Add(transition);
            this.transitions[state] = transitions;
        }

        private void SetParentStateMachine(IEventDelegate parentStateMachine, IState state)
        {
            if (this.parentStateMachine != null && this.parentStateMachine != parentStateMachine)
                throw new InvalidEventTransitionException(state, this);

            this.parentStateMachine = parentStateMachine;
        }

        internal IEnumerable<IInvokableTransition<TEventData>> GetTransitionsFromState(IState state)
        {
            OptimizingList<IInvokableTransition<TEventData>> transitions;
            if (!this.transitions.TryGetValue(state, out transitions))
                return Enumerable.Empty<IInvokableTransition<TEventData>>();

            return transitions.GetEnumerable();
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
        /// <param name="eventData">Event data to associate with this event</param>
        /// <returns>True if the event could be fired.</returns>
        public bool TryFire(TEventData eventData)
        {
            return this.RequestEventFireFromEvent(eventData, EventFireMethod.TryFire);
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
            this.RequestEventFireFromEvent(eventData, EventFireMethod.Fire);
        }

        void IEvent.Fire()
        {
            this.Fire(default(TEventData));
        }

        bool IEvent.TryFire()
        {
            return this.TryFire(default(TEventData));
        }

        private bool RequestEventFireFromEvent(TEventData eventData, EventFireMethod eventFireMethod)
        {
            if (this.parentStateMachine == null)
            {
                if (eventFireMethod == EventFireMethod.Fire)
                    throw new InvalidEventSetupException(this);
                else
                    return false;
            }

            return this.parentStateMachine.RequestEventFireFromEvent(this, eventData, eventFireMethod);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object</returns>
        [ExcludeFromCoverage]
        public override string ToString()
        {
            return $"<Event Name={this.Name ?? "(unnamed)"}>";
        }
    }   
}

