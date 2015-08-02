using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    internal interface IStateDelegate<TState>
    {
        TState InitialState { get; }
        TState CurrentState { get; }
        void ForceTransition(TState pretendFromState, TState pretendToState, TState toState, IEvent @event);
    }
}
