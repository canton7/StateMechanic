using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    internal interface IEventDelegate : IStateMachine
    {
        bool RequestEventFireFromEvent(IEvent sourceEvent, Func<IState, bool> invoker, EventFireMethod eventFireMethod);
    }
}
