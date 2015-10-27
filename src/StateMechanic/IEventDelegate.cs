using System;

namespace StateMechanic
{
    internal interface IEventDelegate : IStateMachine
    {
        bool RequestEventFireFromEvent(EventFireData eventFireData);
    }
}
