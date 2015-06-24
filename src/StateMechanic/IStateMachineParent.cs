using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    internal interface IStateMachineParent
    {
        void TransitionBegan();
        void TransitionEnded();
        void EnqueueEventFire(Func<IState, bool> invoker);
        void FireQueuedEvents();
    }
}
