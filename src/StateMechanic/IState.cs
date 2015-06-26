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
        IStateMachine ParentStateMachine { get; }
        IReadOnlyList<ITransition> Transitions { get; }
    }

    internal interface IState<TState> : IState where TState: IState<TState>
    {
        new IStateMachine<TState> ParentStateMachine { get; }
        IStateMachine<TState> ChildStateMachine { get; }
        void AddTransition(ITransition transition);
        void FireOnEntry(StateHandlerInfo<TState> info);
        void FireOnExit(StateHandlerInfo<TState> info);
        void Reset();
    }
}
