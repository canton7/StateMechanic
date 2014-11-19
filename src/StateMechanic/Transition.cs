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
        public readonly TState From;
        public readonly TState To;
        public readonly TEvent Evt;
        public TTransitionHandler Handler;

        public TransitionInner(TState from, TState to, TEvent evt)
        {
            this.From = from;
            this.To = to;
            this.Evt = evt;
        }

        public void WithHandler(TTransitionHandler handler)
        {
            this.Handler = handler;
        }
    }

    public class Transition<TState> : ITransition
    {
        private readonly TransitionInner<TState, Event, TransitionHandler<TState>> innerTransition;

        public TState From { get { return this.innerTransition.From; } }
        public TState To { get { return this.innerTransition.To; } }

        public Transition(TState from, TState to, Event evt)
        {
            this.innerTransition = new TransitionInner<TState, Event, TransitionHandler<TState>>(from, to, evt);
        }

        public void WithHandler(TransitionHandler<TState> handler)
        {
            this.innerTransition.WithHandler(handler);
        }

        public bool CanInvoke()
        {
            return true;
        }

        public void Invoke()
        {
            if (this.innerTransition.Handler != null)
            {
                var transitionInfo = new TransitionInfo<TState>(this.innerTransition.From, this.innerTransition.To, this.innerTransition.Evt);
                this.innerTransition.Handler(transitionInfo);
            }
        }
    }

    public class Transition<TState, TEventData> : ITransition<TEventData>
    {
        private readonly TransitionInner<TState, Event<TEventData>, TransitionHandler<TState, TEventData>> innerTransition;

        public TState From { get { return this.innerTransition.From; } }
        public TState To { get { return this.innerTransition.To; } }

        public Transition(TState from, TState to, Event<TEventData> evt)
        {
            this.innerTransition = new TransitionInner<TState, Event<TEventData>, TransitionHandler<TState, TEventData>>(from, to, evt);
        }

        public void WithHandler(TransitionHandler<TState, TEventData> handler)
        {
            this.innerTransition.WithHandler(handler);
        }

        public bool CanInvoke()
        {
            return true;
        }

        public void Invoke(TEventData eventData)
        {
            if (this.innerTransition.Handler != null)
            {
                var transitionInfo = new TransitionInfo<TState, TEventData>(this.innerTransition.From, this.innerTransition.To, this.innerTransition.Evt, eventData);
                this.innerTransition.Handler(transitionInfo);
            }
        }
    }
}
