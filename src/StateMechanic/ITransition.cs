using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public interface ITransition<out TState> where TState : class, IState
    {
        TState From { get; }
        TState To { get; }
        IEvent Event { get; }
    }
}
