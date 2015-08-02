using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    /// <summary>
    /// State machine synchronizes which achieves thread-safety by synchronous locking
    /// </summary>
    public class LockingStateMachineSynchronizer : IStateMachineSynchronizer
    {
        private readonly object lockObject = new object();

        /// <summary>
        /// Method called whenever an event's Fire or TryFire method is invoked
        /// </summary>
        /// <param name="invoker">Delegate which, when invoked, will attempt to fire the event.</param>
        /// <param name="fireMethod">Value indicating whether the event was fired using Fire or TryFire</param>
        /// <returns>The value to return from the event's TryFire method, if that was used to fire the event</returns>
        public bool FireEvent(Func<bool> invoker, EventFireMethod fireMethod)
        {
            lock (this.lockObject)
            {
                return invoker();
            }
        }

        /// <summary>
        /// Method called when ForceTransition is invoked
        /// </summary>
        /// <param name="invoker">Method which will cause the forced transition to occur</param>
        public void ForceTransition(Action invoker)
        {
            lock (this.lockObject)
            {
                invoker();
            }
        }

        /// <summary>
        /// Method called when Reset is invoked
        /// </summary>
        /// <param name="invoker">Method which will cause the reset to occur</param>
        public void Reset(Action invoker)
        {
            lock (this.lockObject)
            {
                invoker();
            }
        }
    }
}
