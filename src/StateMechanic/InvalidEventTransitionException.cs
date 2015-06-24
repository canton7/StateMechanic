using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public class InvalidEventTransitionException : Exception
    {
        public IState From { get; private set; }
        public IEvent Event { get; private set; }

        internal InvalidEventTransitionException(IState from, IEvent evt)
            : base(String.Format("Unable to create transition from state {0} on event {1}, as state {0} does not belong to the same state machine as event {1}, or to a child state machine of event {1}", from.Name, evt.Name))
        {
            this.From = from;
            this.Event = evt;
        }
    }
}
