using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    /// <summary>
    /// An event, which can be fired to trigger a transition from one state to antoher
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// Gets the name assigned to this event
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the state machine associated with this event. This event can be used to trigger transitions on its parent state machine, or any of its child state machines
        /// </summary>
        IStateMachine ParentStateMachine { get; }

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
        bool TryFire();

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
        void Fire();
    }
}
