using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    internal interface ITransitionDelegate<TState> where TState : IState
    {
        void UpdateCurrentState(TState from, TState state, IEvent evt, bool isInnerTransition);
    }
}
