using System;

namespace StateMechanic
{
    internal interface ITransitionDelegate<TState> where TState : IState
    {
        void CoordinateTransition(TState from, TState to, IEvent @event, bool isInnerTransition, Action handlerInvoker);
    }
}
