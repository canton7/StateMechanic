using System;

namespace StateMechanic
{
    // Oh the hoops we jump through to have Transition<T> public...
    internal interface ITransitionInner<TState, TEvent, TTransitionInfo> where TState : IState
    {
        TState From { get; }
        TState To { get; }
        TEvent Event { get; }
        bool IsInnerTransition { get; }
        Action<TTransitionInfo> Handler { get; set; }
        Func<TTransitionInfo, bool> Guard { get; set; }
        bool TryInvoke(TTransitionInfo transitionInfo);
    }
}
