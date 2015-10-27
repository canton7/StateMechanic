using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    internal interface IEventInternal : IEvent
    {
        bool FireEventFromStateMachine(IState currentState, object eventData);
    }
}
