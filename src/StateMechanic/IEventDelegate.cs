using System;

namespace StateMechanic
{
    internal interface IEventDelegate : IStateMachine
    {
        bool RequestEventFireFromEvent(IEvent sourceEvent, Func<IState, bool> invoker, EventFireMethod eventFireMethod);
    }
}
