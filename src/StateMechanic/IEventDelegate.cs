﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    internal interface IEventDelegate : IStateMachine
    {
        IState CurrentState { get; }

        bool RequestEventFire(Func<IState, bool> invoker);
    }
}
