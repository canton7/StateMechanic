using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    internal interface IInvocableTransition
    {
        bool TryInvoke();
    }

    internal interface IInvocableTransition<TEventData>
    {
        bool TryInvoke(TEventData eventData);
    }
}
