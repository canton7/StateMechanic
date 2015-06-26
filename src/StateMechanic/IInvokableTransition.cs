using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    internal interface IInvokableTransition
    {
        bool TryInvoke();
    }

    internal interface IInvokableTransition<TEventData>
    {
        bool TryInvoke(TEventData eventData);
    }
}
