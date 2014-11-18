using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public delegate void TransitionHandler<TState>(TransitionInfo<TState> info);
    public delegate void TransitionHandler<TState, TEventData>(TransitionInfo<TState, TEventData> info);

    internal class TransitionInner<TState, TEvent, TTransitionHandler>
    {
        private readonly TState from;
        private readonly TState to;
        private readonly TEvent evt;
        private TTransitionHandler handler;

        public TransitionInner(TState from, TState to, TEvent evt)
        {
            this.from = from;
            this.to = to;
            this.evt = evt;
        }

        public void WithHandler(TTransitionHandler handler)
        {
            this.handler = handler;
        }
    }

    public class Transition<TState> : ITransition<TState>
    {
        private readonly TransitionInner<TState, Event, TransitionHandler<TState>> transitionInner;

        public Transition(TState from, TState to, Event evt)
        {
            this.transitionInner = new TransitionInner<TState, Event, TransitionHandler<TState>>(from, to, evt);
        }

        public void WithHandler(TransitionHandler<TState> handler)
        {
            this.transitionInner.WithHandler(handler);
        }
    }

    public class Transition<TState, TEventData> : ITransition<TState>
    {
        private readonly TransitionInner<TState, Event<TEventData>, TransitionHandler<TState, TEventData>> transitionInner;

        public Transition(TState from, TState to, Event<TEventData> evt)
        {
            this.transitionInner = new TransitionInner<TState, Event<TEventData>, TransitionHandler<TState, TEventData>>(from, to, evt);
        }

        public void WithHandler(TransitionHandler<TState, TEventData> handler)
        {
            this.transitionInner.WithHandler(handler);
        }
    }
}
