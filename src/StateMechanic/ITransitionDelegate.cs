using System;

namespace StateMechanic
{
    internal interface ITransitionDelegate<TState> where TState : IState
    {
        void CoordinateTransition<TTransitionInfo>(TTransitionInfo transitionInfo, Action<TTransitionInfo> handler)
            where TTransitionInfo : ITransitionInfo<TState>;
    }
}
