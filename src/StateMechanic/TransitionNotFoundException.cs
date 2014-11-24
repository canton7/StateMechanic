using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public class TransitionNotFoundException : Exception
    {
        public IState FromState { get; private set; }
        public IEvent Event { get; private set; }

        public TransitionNotFoundException(IState fromState, IEvent evt)
            : base(String.Format("Transition not found from state {0} on event {1}, or transition was otherwise denied (maybe a forced transition was in progress?)", fromState.Name, evt.Name))
        {
            this.FromState = fromState;
            this.Event = evt;
        }
    }
}
