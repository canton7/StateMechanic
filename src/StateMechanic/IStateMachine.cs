using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public interface IStateMachine
    {
        string Name { get; }
        IState CurrentState { get; }
        bool IsChildOf(IStateMachine parentStateMachine);
    }

    internal interface IStateMachine<TState>
    {
        TState CurrentStateRecursive { get; }
        bool RequestEventFire(Func<IState, bool> invoker);
    }
}
