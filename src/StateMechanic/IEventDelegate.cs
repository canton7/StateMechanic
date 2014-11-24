using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    internal interface IEventDelegate
    {
        IState CurrentState { get; }

        bool RequestEventFire(Func<IState, Action<Action<TransitionInvocationState>>, bool> invoker);
    }
}
