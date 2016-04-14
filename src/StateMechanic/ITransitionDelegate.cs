using System;

namespace StateMechanic
{
    internal interface ITransitionDelegate<TState> where TState : IState
    {
        void CoordinateTransition<TTransitionInfo>(TState from, TState to, IEvent @event, bool isInnerTransition, Action<TTransitionInfo> handler, TTransitionInfo transitionInfo);
    }
}
