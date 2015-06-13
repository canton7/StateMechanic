using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    internal interface ITransitionGuard
    {
        bool CanInvoke();
    }

    internal interface IInvocableTransition : ITransitionGuard
    {
        void Invoke();
    }

    internal interface IInvocableTransition<TEventData> : ITransitionGuard
    {
        void Invoke(TEventData eventData);
    }
}
