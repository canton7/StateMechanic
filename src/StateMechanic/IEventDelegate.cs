using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    internal delegate void EventFirer(Action<TransitionInvocationState> transitionInvoker);

    internal interface IEventDelegate
    {
        IState CurrentState { get; }

        bool RequestEventFire(Func<IState, EventFirer, bool> invoker);
    }
}
