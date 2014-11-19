using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    internal interface ITransitionRepository<TState> where TState : IState<TState>
    {
        void AddTransition(Event evt, Transition<TState> transition);

        void AddTransition<TEventData>(Event<TEventData> evt, Transition<TState, TEventData> transition);
    }
}
