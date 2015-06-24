﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    internal class TransitionBuilder<TState> : ITransitionBuilder<TState> where TState : IState<TState>
    {
        private readonly TState fromState;
        private readonly Event evt;
        private readonly ITransitionDelegate<TState> transitionDelegate;

        public TransitionBuilder(TState fromState, Event evt, ITransitionDelegate<TState> transitionDelegate)
        {
            this.fromState = fromState;
            this.evt = evt;
            this.transitionDelegate = transitionDelegate;
        }

        ITransition<TState> ITransitionBuilder<TState>.To(TState state)
        {
            var transition = new Transition<TState>(this.fromState, state, this.evt, this.transitionDelegate);
            this.evt.AddTransition(this.fromState, transition);
            return transition;
        }
    }

    internal class TransitionBuilder<TState, TEventData> : ITransitionBuilder<TState, TEventData> where TState : IState<TState>
    {
        private readonly TState fromState;
        private readonly Event<TEventData> evt;
        private readonly ITransitionDelegate<TState> transitionDelegate;

        public TransitionBuilder(TState fromState, Event<TEventData> evt, ITransitionDelegate<TState> transitionDelegate)
        {
            this.fromState = fromState;
            this.evt = evt;
            this.transitionDelegate = transitionDelegate;
        }

        ITransition<TState, TEventData> ITransitionBuilder<TState, TEventData>.To(TState state)
        {
            var transition = new Transition<TState, TEventData>(this.fromState, state, this.evt, this.transitionDelegate);
            this.evt.AddTransition(this.fromState, transition);
            return transition;
        }
    }
}
