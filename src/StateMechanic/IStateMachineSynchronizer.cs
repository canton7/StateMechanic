using System;

namespace StateMechanic
{
    /// <summary>
    /// Helper which can add thread-safety to a StateMachine
    /// </summary>
    public interface IStateMachineSynchronizer
    {
        /// <summary>
        /// Method called whenever an event's Fire or TryFire method is invoked
        /// </summary>
        /// <param name="invoker">Delegate which, when invoked, will attempt to fire the event.</param>
        /// <param name="fireMethod">Value indicating whether the event was fired using Fire or TryFire</param>
        /// <returns>The value to return from the event's TryFire method, if that was used to fire the event</returns>
        /// <remarks>
        /// If the transition could not be found, then the behaviour of 'invoker' depends on whether the event was fired
        /// using Fire or TryFire. If Fire was used, then 'invoker' will throw a <see cref="TransitionNotFoundException"/>.
        /// If TryFire was used, then 'invoker' will return false.
        /// 
        /// This method is also invoked for events which are fired from exit/entry/transition handlers. How you deal with
        /// these is up to you. If you execute them synchronously, then they will be queued using the same internal
        /// queue as is used when no synchronizer is present, and executed synchronously when then transition completes.
        /// 
        /// Synchronizers may work by scheduling new event executions in a non-blocking fashion. In this case, it is
        /// recommended that you return 'true' (and remember that TryFire will always succeed).
        /// </remarks>
        bool FireEvent(Func<bool> invoker, EventFireMethod fireMethod);

        /// <summary>
        /// Method called when ForceTransition is invoked
        /// </summary>
        /// <param name="invoker">Method which will cause the forced transition to occur</param>
        void ForceTransition(Action invoker);

        /// <summary>
        /// Method called when Reset is invoked
        /// </summary>
        /// <param name="invoker">Method which will cause the reset to occur</param>
        void Reset(Action invoker);
    }
}
