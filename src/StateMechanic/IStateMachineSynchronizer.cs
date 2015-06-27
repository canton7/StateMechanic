using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public interface IStateMachineSynchronizer
    {
        bool FireEvent(Func<bool> invoker);
        void ForceTransition(Action invoker);
        void Reset(Action invoker);
    }
}
