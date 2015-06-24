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
        IStateMachine StateMachine { get; }
    }

    internal interface IState<TState> : IState
    {
        IStateMachine<TState> ChildStateMachine { get; }
        void FireOnEntry(StateHandlerInfo<TState> info);
        void FireOnExit(StateHandlerInfo<TState> info);
    }
}
