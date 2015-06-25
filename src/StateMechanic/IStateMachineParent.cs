using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    internal interface IStateMachineParent<TState>
    {
        IStateMachine StateMachine { get; }
        bool ExecutingTransition { get; set; }
        StateMachineFaultInfo Fault { get; set; }
        void EnqueueEventFire(Func<bool> invoker);
        void FireQueuedEvents();
        void OnRecursiveTransition(TState from, TState to, IEvent evt, bool isInnerTransition);
        void OnRecursiveTransitionNotFound(TState from, IEvent evt);
    }
}
