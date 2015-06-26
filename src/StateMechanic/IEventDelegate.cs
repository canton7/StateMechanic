using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    internal interface IEventDelegate : IStateMachine
    {
        bool RequestEventFire(IEvent sourceEvent, Func<IState, bool> invoker, bool throwIfNotFound);
    }
}
