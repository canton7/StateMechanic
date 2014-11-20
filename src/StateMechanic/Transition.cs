using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public delegate void TransitionHandler<TState>(TransitionInfo<TState> info);
    public delegate void TransitionHandler<TState, TEventData>(TransitionInfo<TState, TEventData> info);

    internal class TransitionInner<TState, TEvent, TTransitionHandler> where TState : IState<TState> where TEvent : IEvent
    {
        public readonly TState From;
        public readonly TState To;
        public readonly TEvent Event;
        public readonly ITransitionDelegate<TState> transitionDelegate;

        public TTransitionHandler Handler;
        public Func<bool> Guard;

        public TransitionInner(TState from, TState to, TEvent evt, ITransitionDelegate<TState> transitionRepository)
        {
            this.From = from;
            this.To = to;
            this.Event = evt;
            this.transitionDelegate = transitionRepository;
        }

        public void Invoke(Action<TTransitionHandler> transitionHandlerInvoker, TransitionInvocationState transitionInvocationState)
        {
            var stateHandlerInfo = new StateHandlerInfo<TState>(this.From, this.To, this.Event);

            if (this.transitionDelegate.ShouldCallExitHandler(transitionInvocationState))
                this.From.FireOnExit(stateHandlerInfo);

            if (this.transitionDelegate.HasOtherEventBeenFired(transitionInvocationState))
                return;

            if (this.Handler != null)
            {
               transitionHandlerInvoker(this.Handler);

               if (this.transitionDelegate.HasOtherEventBeenFired(transitionInvocationState))
                   return;
            }

            this.transitionDelegate.UpdateCurrentState(this.To);

            this.To.FireOnEntry(stateHandlerInfo);
        }

        public bool CanInvoke()
        {
            return this.Guard == null || this.Guard();
        }
    }

    public class Transition<TState> : ITransition where TState : IState<TState>
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
            this.innerTransition = new TransitionInner<TState, Event, TransitionHandler<TState>>(from, to, evt, transitionRepository);
        }

        public Transition<TState> WithHandler(TransitionHandler<TState> handler)
        {
            this.Handler = handler;
            return this;
        }

        public Transition<TState> WithGuard(Func<bool> guard)
        {
            this.Guard = guard;
            return this;
        }

        public bool CanInvoke()
        {
            return this.innerTransition.CanInvoke();
        }

        void ITransition.Invoke(TransitionInvocationState transitionInvocationState)
        {
            this.innerTransition.Invoke(handler =>
            {
                var transitionInfo = new TransitionInfo<TState>(this.innerTransition.From, this.innerTransition.To, this.innerTransition.Event);
                handler(transitionInfo);
            }, transitionInvocationState);
        }
    }

    public class Transition<TState, TEventData> : ITransition<TEventData> where TState : IState<TState>
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

        internal Transition(TState from, TState to, Event<TEventData> evt, ITransitionDelegate<TState> transitionRepository)
        {
            this.innerTransition = new TransitionInner<TState, Event<TEventData>, TransitionHandler<TState, TEventData>>(from, to, evt, transitionRepository);
        }

        public Transition<TState, TEventData> WithHandler(TransitionHandler<TState, TEventData> handler)
        {
            this.Handler = handler;
            return this;
        }

        public Transition<TState, TEventData> WithGuard(Func<bool> guard)
        {
            this.Guard = guard;
            return this;
        }

        public bool CanInvoke()
        {
            return this.innerTransition.CanInvoke();
        }

        void ITransition<TEventData>.Invoke(TEventData eventData, TransitionInvocationState transitionInvocationState)
        {
            this.innerTransition.Invoke(handler =>
            {
                var transitionInfo = new TransitionInfo<TState, TEventData>(this.innerTransition.From, this.innerTransition.To, this.innerTransition.Event, eventData);
                handler(transitionInfo);
            }, transitionInvocationState);
        }
    }
}
