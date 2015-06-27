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
        IState CurrentStateRecursive { get; }
        IState InitialState { get; }
        IReadOnlyList<IState> States { get; }
        bool IsChildOf(IStateMachine parentStateMachine);
        bool IsInState(IState state);
    }

    internal interface IStateMachine<TState> : IStateMachine, ITransitionDelegate<TState>, IEventDelegate where TState : class, IState<TState>
    {
        new TState CurrentStateRecursive { get; }
    }
}
