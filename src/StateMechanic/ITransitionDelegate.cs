﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    internal interface ITransitionDelegate<TState> : IStateMachine where TState : IState<TState>
    {
        void AddTransition(Event evt, Transition<TState> transition);

        void AddTransition<TEventData>(Event<TEventData> evt, Transition<TState, TEventData> transition);

        void UpdateCurrentState(TState state);

        void TransitionBegan();
        void TransitionEnded();
    }
}