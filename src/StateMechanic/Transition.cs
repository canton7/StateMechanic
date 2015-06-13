using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public delegate void TransitionHandler<TState>(TransitionInfo<TState> info);
    public delegate void TransitionHandler<TState, TEventData>(TransitionInfo<TState> info, TEventData eventData);

    internal class TransitionInner<TState, TEvent, TTransitionHandler> where TState : IState<TState> where TEvent : IEvent
    {
        public readonly TState From;
        public readonly TState To;
        public readonly TEvent Event;
        private readonly ITransitionDelegate<TState> transitionDelegate;
        private readonly bool isInnerTransition;

        public TTransitionHandler Handler;
        public Func<bool> Guard;

        public TransitionInner(TState from, TState to, TEvent evt, ITransitionDelegate<TState> transitionRepository, bool isInnerTransition)
        {
            if (!from.BelongsToSameStateMachineAs(to))
                throw new InvalidTransitionException(from, to, transitionRepository);

            this.From = from;
            this.To = to;
            this.Event = evt;
            this.transitionDelegate = transitionRepository;
            this.isInnerTransition = isInnerTransition;
        }

        public void Invoke(Action<TTransitionHandler> transitionHandlerInvoker)
        {
            var stateHandlerInfo = new StateHandlerInfo<TState>(this.From, this.To, this.Event);

            if (!this.isInnerTransition)
                this.transitionDelegate.SetTransitionBegin();

            try
            {
                if (!this.isInnerTransition)
                    this.From.FireOnExit(stateHandlerInfo);

                if (this.Handler != null)
                {
                    transitionHandlerInvoker(this.Handler);
                }

                this.transitionDelegate.UpdateCurrentState(this.To);
            }
            finally
            {
                this.transitionDelegate.SetTransitionEnd();
            }

            if (!this.isInnerTransition)
                this.To.FireOnEntry(stateHandlerInfo);
        }

        public bool CanInvoke()
        {
            return this.Guard == null || this.Guard();
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
        public Func<bool> Guard
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

        public ITransition<TState> WithGuard(Func<bool> guard)
        {
            this.Guard = guard;
            return this;
        }

        public bool CanInvoke()
        {
            return this.innerTransition.CanInvoke();
        }

        public void Invoke()
        {
            this.innerTransition.Invoke(handler =>
            {
                var transitionInfo = new TransitionInfo<TState>(this.innerTransition.From, this.innerTransition.To, this.innerTransition.Event);
                handler(transitionInfo);
            });
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
        public Func<bool> Guard
        {
            get { return this.innerTransition.Guard; }
            set { this.innerTransition.Guard = value; }
        }

        public Transition(TState from, TState to, Event<TEventData> evt, ITransitionDelegate<TState> transitionRepository)
        {
            this.innerTransition = new TransitionInner<TState, Event<TEventData>, TransitionHandler<TState, TEventData>>(from, to, evt, transitionRepository, isInnerTransition: false);
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

        public ITransition<TState, TEventData> WithGuard(Func<bool> guard)
        {
            this.Guard = guard;
            return this;
        }

        public bool CanInvoke()
        {
            return this.innerTransition.CanInvoke();
        }

        public void Invoke(TEventData eventData)
        {
            this.innerTransition.Invoke(handler =>
            {
                var transitionInfo = new TransitionInfo<TState>(this.innerTransition.From, this.innerTransition.To, this.innerTransition.Event);
                handler(transitionInfo, eventData);
            });
        }
    }
}
