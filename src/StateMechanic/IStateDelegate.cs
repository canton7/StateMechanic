using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    internal interface IStateDelegate<TState> : ITransitionDelegate<TState>, IStateMachineParent where TState : IState<TState>
    {
    }
}
