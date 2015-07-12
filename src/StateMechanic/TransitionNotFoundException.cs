using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    /// <summary>
    /// Indicates that no transition exists from the current state on the given event
    /// </summary>
    public class TransitionNotFoundException : Exception
    {
        /// <summary>
        /// Gets the current state, from which a transtion was requested
        /// </summary>
        public IState From { get; private set; }

        /// <summary>
        /// Gets the event which was fired
        /// </summary>
        public IEvent Event { get; private set; }

        internal TransitionNotFoundException(IState from, IEvent evt)
            : base(String.Format("Could not find a transition which we could invoke from state {0} (or any of its children) on event {1}", from.Name, evt.Name))
        {
            this.From = from;
            this.Event = evt;
        }
    }
}
