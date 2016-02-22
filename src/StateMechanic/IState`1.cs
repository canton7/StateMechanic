using System.Collections.Generic;

namespace StateMechanic
{
    internal interface IState<TState> : IState where TState : class, IState<TState>
    {
        new IStateMachine<TState> ParentStateMachine { get; }
        new IStateMachine<TState> ChildStateMachine { get; }
        new IReadOnlyList<ITransition<TState>> Transitions { get; }
        new IReadOnlyList<IStateGroup<TState>> Groups { get; }
        void AddTransition(ITransition<TState> transition);
        void FireEntryHandler(StateHandlerInfo<TState> info);
        void FireExitHandler(StateHandlerInfo<TState> info);
    }
}
