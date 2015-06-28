using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    // Oh the hoops we jump through to have Transition<T> public...
    internal interface ITransitionInner<TState, TEvent, TTransitionInfo> where TState : class, IState
    {
        TState From { get; }
        TState To { get; }
        TEvent Event { get; }
        bool IsInnerTransition { get; }
        Action<TTransitionInfo> Handler { get; set; }
        Func<TTransitionInfo, bool> Guard { get; set; }
        bool TryInvoke(TTransitionInfo transitionInfo);
    }

    internal class TransitionInner<TState, TEvent, TTransitionInfo> : ITransitionInner<TState, TEvent, TTransitionInfo> where TState : class, IState<TState> where TEvent : IEvent
    {
        public TState From { get; private set; }
        public TState To { get; private set; }
        public TEvent Event { get; private set; }
        public bool IsInnerTransition { get; private set; }
        private readonly ITransitionDelegate<TState> transitionDelegate;

        public Action<TTransitionInfo> Handler { get; set; }
        public Func<TTransitionInfo, bool> Guard { get; set; }

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
            this.IsInnerTransition = isInnerTransition;
        }

        public bool TryInvoke(TTransitionInfo transitionInfo)
        {
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

            if (!this.IsInnerTransition)
            {
                try
                {
                    this.From.FireExitHandler(stateHandlerInfo);
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
                    this.Handler(transitionInfo);
                }
                catch (Exception e)
                {
                    throw new InternalTransitionFaultException(this.From, this.To, this.Event, FaultedComponent.TransitionHandler, e);
                }
            }
                
            this.transitionDelegate.UpdateCurrentState(this.From, this.To, this.Event, this.IsInnerTransition);

            if (!this.IsInnerTransition)
            {
                try
                {
                    this.To.FireEntryHandler(stateHandlerInfo);
                }
                catch (Exception e)
                {
                    throw new InternalTransitionFaultException(this.From, this.To, this.Event, FaultedComponent.EntryHandler, e);
                }
            }

            return true;
        }
    }

    public class Transition<TState> : ITransition<TState>, IInvokableTransition where TState : class, IState
    {
        private readonly ITransitionInner<TState, Event, TransitionInfo<TState>> innerTransition;

        public TState From { get { return this.innerTransition.From; } }
        public TState To { get { return this.innerTransition.To; } }
        public Event Event { get { return this.innerTransition.Event; } }
        IEvent ITransition<TState>.Event { get { return this.innerTransition.Event; } }
        public bool IsInnerTransition { get { return this.innerTransition.IsInnerTransition; } }
        public Action<TransitionInfo<TState>> Handler
        {
            get { return this.innerTransition.Handler; }
            set { this.innerTransition.Handler = value; }
        }
        public Func<TransitionInfo<TState>, bool> Guard
        {
            get { return this.innerTransition.Guard; }
            set { this.innerTransition.Guard = value; }
        }

        internal Transition(ITransitionInner<TState, Event, TransitionInfo<TState>> innerTransition)
        {
            this.innerTransition = innerTransition;
        }

        public Transition<TState> WithHandler(Action<TransitionInfo<TState>> handler)
        {
            this.Handler = handler;
            return this;
        }

        public Transition<TState> WithGuard(Func<TransitionInfo<TState>, bool> guard)
        {
            this.Guard = guard;
            return this;
        }

        bool IInvokableTransition.TryInvoke()
        {
            var transitionInfo = new TransitionInfo<TState>(this.From, this.To, this.Event, this.IsInnerTransition);
            return this.innerTransition.TryInvoke(transitionInfo);
        }
    }

    public class Transition<TState, TEventData> : ITransition<TState>, IInvokableTransition<TEventData> where TState : class, IState
    {
        private readonly ITransitionInner<TState, Event<TEventData>, TransitionInfo<TState, TEventData>> innerTransition;

        public TState From { get { return this.innerTransition.From; } }
        public TState To { get { return this.innerTransition.To; } }
        public Event<TEventData> Event { get { return this.innerTransition.Event; } }
        IEvent ITransition<TState>.Event { get { return this.innerTransition.Event; } }
        public bool IsInnerTransition { get { return this.innerTransition.IsInnerTransition; } }
        public Action<TransitionInfo<TState, TEventData>> Handler
        {
            get { return this.innerTransition.Handler; }
            set { this.innerTransition.Handler = value; }
        }
        public Func<TransitionInfo<TState, TEventData>, bool> Guard
        {
            get { return this.innerTransition.Guard; }
            set { this.innerTransition.Guard = value; }
        }

        internal Transition(ITransitionInner<TState, Event<TEventData>, TransitionInfo<TState, TEventData>> innerTransition) 
        {
            this.innerTransition = innerTransition;
        }

        public Transition<TState, TEventData> WithHandler(Action<TransitionInfo<TState, TEventData>> handler)
        {
            this.Handler = handler;
            return this;
        }

        public Transition<TState, TEventData> WithGuard(Func<TransitionInfo<TState, TEventData>, bool> guard)
        {
            this.Guard = guard;
            return this;
        }

        bool IInvokableTransition<TEventData>.TryInvoke(TEventData eventData)
        {
            var transitionInfo = new TransitionInfo<TState, TEventData>(this.From, this.To, this.Event, eventData, this.IsInnerTransition);
            return this.innerTransition.TryInvoke(transitionInfo);
        }
    }

    internal static class Transition
    {
        internal static Transition<TState> Create<TState>(TState from, TState to, Event evt, ITransitionDelegate<TState> transitionDelegate) where TState : class, IState<TState>
        {
            return new Transition<TState>(new TransitionInner<TState, Event, TransitionInfo<TState>>(from, to, evt, transitionDelegate, isInnerTransition: false));
        }

        internal static Transition<TState> CreateInner<TState>(TState fromAndTo, Event evt, ITransitionDelegate<TState> transitionDelegate) where TState : class, IState<TState>
        {
            return new Transition<TState>(new TransitionInner<TState, Event, TransitionInfo<TState>>(fromAndTo, fromAndTo, evt, transitionDelegate, isInnerTransition: true));
        }

        internal static Transition<TState, TEventData> Create<TState, TEventData>(TState from, TState to, Event<TEventData> evt, ITransitionDelegate<TState> transitionDelegate) where TState : class, IState<TState>
        {
            return new Transition<TState, TEventData>(new TransitionInner<TState, Event<TEventData>, TransitionInfo<TState, TEventData>>(from, to, evt, transitionDelegate, isInnerTransition: false));
        }

        internal static Transition<TState, TEventData> CreateInner<TState, TEventData>(TState fromAndTo, Event<TEventData> evt, ITransitionDelegate<TState> transitionDelegate) where TState : class, IState<TState>
        {
            return new Transition<TState, TEventData>(new TransitionInner<TState, Event<TEventData>, TransitionInfo<TState, TEventData>>(fromAndTo, fromAndTo, evt, transitionDelegate, isInnerTransition: true));
        }
    }
}
