using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public interface IEvent
    {
        string Name { get; }
        IStateMachine ParentStateMachine { get; }
        bool TryFire();
        void Fire();
    }
}
