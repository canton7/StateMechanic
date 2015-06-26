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
        IStateMachine ChildStateMachine { get; }
        IReadOnlyList<ITransition<IState>> Transitions { get; }
    }

    internal interface IState<TState> : IState where TState: class, IState<TState>
    {
        new IStateMachine<TState> ParentStateMachine { get; }
        new IStateMachine<TState> ChildStateMachine { get; }
        new IReadOnlyList<ITransition<TState>> Transitions { get; }
        void AddTransition(ITransition<TState> transition);
        void FireEntryHandler(StateHandlerInfo<TState> info);
        void FireExitHandler(StateHandlerInfo<TState> info);
        void Reset();
    }
}
