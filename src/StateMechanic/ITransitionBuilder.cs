using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public interface ITransitionBuilder<TState> where TState : class, IState
    {
        Transition<TState> To(TState state);
    }

    public interface ITransitionBuilder<TState, TEventData> where TState : class, IState
    {
        Transition<TState, TEventData> To(TState state);
    }
}
