using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    internal interface IStateMachineParent<TState>
    {
        void TransitionBegan();
        void TransitionEnded();
        void EnqueueEventFire(Func<bool> invoker);
        void FireQueuedEvents();
        void OnRecursiveTransition(TState from, TState to, IEvent evt);
    }
}
