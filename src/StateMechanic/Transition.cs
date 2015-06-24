using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public delegate void TransitionHandler<TState>(TransitionInfo<TState, Event> info);
    public delegate void TransitionHandler<TState, TEventData>(TransitionInfo<TState, Event<TEventData>> info, TEventData eventData);

    internal class TransitionInner<TState, TEvent, TTransitionHandler> where TState : IState<TState> where TEvent : IEvent
    {
        public readonly TState From;
        public readonly TState To;
        public readonly TEvent Event;
        private readonly ITransitionDelegate<TState> transitionDelegate;
        private readonly bool isInnerTransition;

        public TTransitionHandler Handler;
        public Func<TransitionInfo<TState, TEvent>, bool> Guard;

        public TransitionInner(TState from, TState to, TEvent evt, ITransitionDelegate<TState> transitionDelegate, bool isInnerTransition)
        {
            if (from.StateMachine != to.StateMachine)
                throw new InvalidTransitionException(from, to, transitionDelegate);

            this.From = from;
            this.To = to;
            this.Event = evt;
            this.transitionDelegate = transitionDelegate;
            this.isInnerTransition = isInnerTransition;
        }

        public bool TryInvoke(Action<TTransitionHandler, TransitionInfo<TState, TEvent>> transitionHandlerInvoker)
        {
            var transitionInfo = new TransitionInfo<TState, TEvent>(this.From, this.To, this.Event, this.isInnerTransition);

            if (this.Guard != null && !this.Guard(transitionInfo))
                return false;

            var stateHandlerInfo = new StateHandlerInfo<TState>(this.From, this.To, this.Event);

            this.transitionDelegate.TransitionBegan();

            try
            {
                if (!this.isInnerTransition)
                    this.From.FireOnExit(stateHandlerInfo);

                if (this.Handler != null)
                    transitionHandlerInvoker(this.Handler, transitionInfo);

                this.transitionDelegate.UpdateCurrentState(this.From, this.To, this.Event);

                if (!this.isInnerTransition)
                    this.To.FireOnEntry(stateHandlerInfo);
            }
            finally
            {
                this.transitionDelegate.TransitionEnded();
            }

            return true;
        }
    }

    internal class Transition<TState> : ITransition<TState>, IInvocableTransition where TState : IState<TState>
    {
        private readonly TransitionInner<TState, Event, TransitionHandler<TState>> innerTransition;

        public TState From { get { return this.innerTransition.From; } }
        public TState To { get { return this.innerTransition.To; } }
        public TransitionHandler<TState> Handler
        {
            get { return this.innerTransition.Handler; }
            set { this.innerTransition.Handler = value; }
        }
        public Func<TransitionInfo<TState, Event>, bool> Guard
        {
            get { return this.innerTransition.Guard; }
            set { this.innerTransition.Guard = value; }
        }

        internal Transition(TState from, TState to, Event evt, ITransitionDelegate<TState> transitionRepository)
        {
            this.innerTransition = new TransitionInner<TState, Event, TransitionHandler<TState>>(from, to, evt, transitionRepository, isInnerTransition: false);
        }

        internal Transition(TState fromAndTo, Event evt, ITransitionDelegate<TState> transitionRepository)
        {
            this.innerTransition = new TransitionInner<TState, Event, TransitionHandler<TState>>(fromAndTo, fromAndTo, evt, transitionRepository, isInnerTransition: true);
        }

        public ITransition<TState> WithHandler(TransitionHandler<TState> handler)
        {
            this.Handler = handler;
            return this;
        }

        public ITransition<TState> WithGuard(Func<TransitionInfo<TState, Event>, bool> guard)
        {
            this.Guard = guard;
            return this;
        }

        public bool TryInvoke()
        {
            return this.innerTransition.TryInvoke((handler, info) => handler(info));
        }
    }

    internal class Transition<TState, TEventData> : ITransition<TState, TEventData>, IInvocableTransition<TEventData> where TState : IState<TState>
    {
        private readonly TransitionInner<TState, Event<TEventData>, TransitionHandler<TState, TEventData>> innerTransition;

        public TState From { get { return this.innerTransition.From; } }
        public TState To { get { return this.innerTransition.To; } }
        public TransitionHandler<TState, TEventData> Handler
        {
            get { return this.innerTransition.Handler; }
            set { this.innerTransition.Handler = value; }
        }
        public Func<TransitionInfo<TState, Event<TEventData>>, bool> Guard
        {
            get { return this.innerTransition.Guard; }
            set { this.innerTransition.Guard = value; }
        }

        public Transition(TState from, TState to, Event<TEventData> evt, ITransitionDelegate<TState> transitionDelegate)
        {
            this.innerTransition = new TransitionInner<TState, Event<TEventData>, TransitionHandler<TState, TEventData>>(from, to, evt, transitionDelegate, isInnerTransition: false);
        }

        internal Transition(TState fromAndTo, Event<TEventData> evt, ITransitionDelegate<TState> transitionRepository)
        {
            this.innerTransition = new TransitionInner<TState, Event<TEventData>, TransitionHandler<TState, TEventData>>(fromAndTo, fromAndTo, evt, transitionRepository, isInnerTransition: true);
        }

        public ITransition<TState, TEventData> WithHandler(TransitionHandler<TState, TEventData> handler)
        {
            this.Handler = handler;
            return this;
        }

        public ITransition<TState, TEventData> WithGuard(Func<TransitionInfo<TState, Event<TEventData>>, bool> guard)
        {
            this.Guard = guard;
            return this;
        }

        public bool TryInvoke(TEventData eventData)
        {
            return this.innerTransition.TryInvoke((handler, info) => handler(info, eventData));
        }
    }
}
