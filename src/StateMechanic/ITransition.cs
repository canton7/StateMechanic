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

    internal interface ITransition : ITransitionGuard
    {
        void Invoke();
    }

    internal interface ITransition<TEventData> : ITransitionGuard
    {
        void Invoke(TEventData eventData);
    }
}
