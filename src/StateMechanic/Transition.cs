using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public delegate void TransitionHandler<TState>(TransitionInfo<TState, Event> info);
    public delegate void TransitionHandler<TState, TEventData>(TransitionInfo<TState, Event<TEventData>> info, TEventData eventData);

    // Oh the hoops we jump through to have Transition<T> public...
    internal interface ITransitionInner<TState, TEvent, TTransitionHandler> where TState : class, IState
    {
        TState From { get; }
        TState To { get; }
        TEvent Event { get; }
        TTransitionHandler Handler { get; set; }
        Func<TransitionInfo<TState, TEvent>, bool> Guard { get; set; }
        bool TryInvoke(Action<TTransitionHandler, TransitionInfo<TState, TEvent>> transitionHandlerInvoker);
    }

    internal class TransitionInner<TState, TEvent, TTransitionHandler> : ITransitionInner<TState, TEvent, TTransitionHandler> where TState : class, IState<TState> where TEvent : IEvent
    {
        public TState From { get; private set; }
        public TState To { get; private set; }
        public TEvent Event { get; private set; }
        private readonly ITransitionDelegate<TState> transitionDelegate;
        private readonly bool isInnerTransition;

        public TTransitionHandler Handler { get; set; }
        public Func<TransitionInfo<TState, TEvent>, bool> Guard { get; set; }

        public TransitionInner(TState from, TState to, TEvent evt, ITransitionDelegate<TState> transitionDelegate, bool isInnerTransition)
        {
            if (from.ParentStateMachine != to.ParentStateMachine)
                throw new InvalidStateTransitionException(from, to);

            if (from.ParentStateMachine != evt.ParentStateMachine && !from.ParentStateMachine.IsChildOf(evt.ParentStateMachine))
                throw new InvalidEventTransitionException(from, evt);

            this.From = from;
            this.To = to;
            this.Event = evt;
            this.transitionDelegate = transitionDelegate;
            this.isInnerTransition = isInnerTransition;
        }

        public bool TryInvoke(Action<TTransitionHandler, TransitionInfo<TState, TEvent>> transitionHandlerInvoker)
        {
            var transitionInfo = new TransitionInfo<TState, TEvent>(this.From, this.To, this.Event, this.isInnerTransition);

            if (this.Guard != null)
            {
                try
                {
                    if (!this.Guard(transitionInfo))
                        return false;
                }
                catch (Exception e)
                {
                    throw new InternalTransitionFaultException(this.From, this.To, this.Event, FaultedComponent.Guard, e);
                }
            }

            var stateHandlerInfo = new StateHandlerInfo<TState>(this.From, this.To, this.Event);

            if (!this.isInnerTransition)
            {
                try
                {
                    this.From.FireOnExit(stateHandlerInfo);
                }
                catch (Exception e)
                {
                    throw new InternalTransitionFaultException(this.From, this.To, this.Event, FaultedComponent.ExitHandler, e);
                }
            }

            if (this.Handler != null)
            {
                try
                {
                    transitionHandlerInvoker(this.Handler, transitionInfo);
                }
                catch (Exception e)
                {
                    throw new InternalTransitionFaultException(this.From, this.To, this.Event, FaultedComponent.TransitionHandler, e);
                }
            }
                
            this.transitionDelegate.UpdateCurrentState(this.From, this.To, this.Event, this.isInnerTransition);

            if (!this.isInnerTransition)
            {
                try
                {
                    this.To.FireOnEntry(stateHandlerInfo);
                }
                catch (Exception e)
                {
                    throw new InternalTransitionFaultException(this.From, this.To, this.Event, FaultedComponent.EntryHandler, e);
                }
            }

            return true;
        }
    }

    public class Transition<TState> : ITransition, IInvokableTransition where TState : class, IState
    {
        private readonly ITransitionInner<TState, Event, TransitionHandler<TState>> innerTransition;

        public TState From { get { return this.innerTransition.From; } }
        public TState To { get { return this.innerTransition.To; } }
        public Event Event { get { return this.innerTransition.Event; } }
        IState ITransition.From { get { return this.innerTransition.From; } }
        IState ITransition.To { get { return this.innerTransition.To; } }
        IEvent ITransition.Event { get { return this.innerTransition.Event; } }
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

        internal Transition(ITransitionInner<TState, Event, TransitionHandler<TState>> innerTransition)
        {
            this.innerTransition = innerTransition;
        }

        public Transition<TState> WithHandler(TransitionHandler<TState> handler)
        {
            this.Handler = handler;
            return this;
        }

        public Transition<TState> WithGuard(Func<TransitionInfo<TState, Event>, bool> guard)
        {
            this.Guard = guard;
            return this;
        }

        bool IInvokableTransition.TryInvoke()
        {
            return this.innerTransition.TryInvoke((handler, info) => handler(info));
        }
    }

    public class Transition<TState, TEventData> : ITransition, IInvokableTransition<TEventData> where TState : class, IState
    {
        private readonly ITransitionInner<TState, Event<TEventData>, TransitionHandler<TState, TEventData>> innerTransition;

        public TState From { get { return this.innerTransition.From; } }
        public TState To { get { return this.innerTransition.To; } }
        public Event<TEventData> Event { get { return this.innerTransition.Event; } }
        IState ITransition.From { get { return this.innerTransition.From; } }
        IState ITransition.To { get { return this.innerTransition.To; } }
        IEvent ITransition.Event { get { return this.innerTransition.Event; } }
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

        internal Transition(ITransitionInner<TState, Event<TEventData>, TransitionHandler<TState, TEventData>> innerTransition) 
        {
            this.innerTransition = innerTransition;
        }

        public Transition<TState, TEventData> WithHandler(TransitionHandler<TState, TEventData> handler)
        {
            this.Handler = handler;
            return this;
        }

        public Transition<TState, TEventData> WithGuard(Func<TransitionInfo<TState, Event<TEventData>>, bool> guard)
        {
            this.Guard = guard;
            return this;
        }

        bool IInvokableTransition<TEventData>.TryInvoke(TEventData eventData)
        {
            return this.innerTransition.TryInvoke((handler, info) => handler(info, eventData));
        }
    }

    internal static class Transition
    {
        internal static Transition<TState> Create<TState>(TState from, TState to, Event evt, ITransitionDelegate<TState> transitionDelegate) where TState : class, IState<TState>
        {
            return new Transition<TState>(new TransitionInner<TState, Event, TransitionHandler<TState>>(from, to, evt, transitionDelegate, isInnerTransition: false));
        }

        internal static Transition<TState> CreateInner<TState>(TState fromAndTo, Event evt, ITransitionDelegate<TState> transitionDelegate) where TState : class, IState<TState>
        {
            return new Transition<TState>(new TransitionInner<TState, Event, TransitionHandler<TState>>(fromAndTo, fromAndTo, evt, transitionDelegate, isInnerTransition: true));
        }

        internal static Transition<TState, TEventData> Create<TState, TEventData>(TState from, TState to, Event<TEventData> evt, ITransitionDelegate<TState> transitionDelegate) where TState : class, IState<TState>
        {
            return new Transition<TState, TEventData>(new TransitionInner<TState, Event<TEventData>, TransitionHandler<TState, TEventData>>(from, to, evt, transitionDelegate, isInnerTransition: false));
        }

        internal static Transition<TState, TEventData> CreateInner<TState, TEventData>(TState fromAndTo, Event<TEventData> evt, ITransitionDelegate<TState> transitionDelegate) where TState : class, IState<TState>
        {
            return new Transition<TState, TEventData>(new TransitionInner<TState, Event<TEventData>, TransitionHandler<TState, TEventData>>(fromAndTo, fromAndTo, evt, transitionDelegate, isInnerTransition: true));
        }
    }
}
