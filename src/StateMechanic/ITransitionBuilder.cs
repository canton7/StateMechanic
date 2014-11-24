using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public interface ITransitionBuilder<TState>
    {
        ITransition<TState> To(TState state);
    }

    public interface ITransitionBuilder<TState, TEventData>
    {
        ITransition<TState, TEventData> To(TState state);
    }
}
