using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public interface IState
    {
        string Name { get; }
    }

    internal interface IState<TState> : IState
    {
        void FireOnEntry(StateHandlerInfo<TState> info);
        void FireOnExit(StateHandlerInfo<TState> info);

        bool RequestEventFire(Func<IState, bool> invoker);

        bool BelongsToSameStateMachineAs(TState otherState);
    }
}
