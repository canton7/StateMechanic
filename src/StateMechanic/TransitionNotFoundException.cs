using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public class TransitionNotFoundException : Exception
    {
        public IState From { get; private set; }
        public IEvent Event { get; private set; }

        public TransitionNotFoundException(IState from, IEvent evt)
            : base(String.Format("Could not find a transition which we would invoke from state {0} (or any of its children) on event {1}", from.Name, evt.Name))
        {
            this.From = from;
            this.Event = evt;
        }
    }
}
