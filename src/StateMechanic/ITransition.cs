using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public interface ITransition
    {
        IState From { get; }
        IState To { get; }
        IEvent Event { get; }
    }
}
