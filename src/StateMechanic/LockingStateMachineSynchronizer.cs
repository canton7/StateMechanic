using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public class LockingStateMachineSynchronizer : IStateMachineSynchronizer
    {
        private readonly object lockObject = new object();

        public bool FireEvent(Func<bool> invoker)
        {
            lock (this.lockObject)
            {
                return invoker();
            }
        }

        public void ForceTransition(Action invoker)
        {
            lock (this.lockObject)
            {
                invoker();
            }
        }

        public void Reset(Action invoker)
        {
            lock (this.lockObject)
            {
                invoker();
            }
        }
    }
}
