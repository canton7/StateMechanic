using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public interface ITransition
    {
        IState From { get; }
        IState To { get; }
        IEvent Event { get; }
    }

    public interface ITransition<TState> : ITransition
    {
        new TState From { get; }
        new TState To { get; }
        TransitionHandler<TState> Handler { get; set; }
        Func<TransitionInfo<TState, Event>, bool> Guard { get; set; }

        ITransition<TState> WithHandler(TransitionHandler<TState> handler);
        ITransition<TState> WithGuard(Func<TransitionInfo<TState, Event>, bool> guard);
    }

    public interface ITransition<TState, TEventData> : ITransition
    {
        new TState From { get; }
        new TState To { get; }
        TransitionHandler<TState, TEventData> Handler { get; set; }
        Func<TransitionInfo<TState, Event<TEventData>>, bool> Guard { get; set; }

        ITransition<TState, TEventData> WithHandler(TransitionHandler<TState, TEventData> handler);
        ITransition<TState, TEventData> WithGuard(Func<TransitionInfo<TState, Event<TEventData>>, bool> guard);
    }
}
