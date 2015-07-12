using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    /// <summary>
    /// Event args for events indicating that the requested transition could not be found
    /// </summary>
    /// <typeparam name="TState">Type of state</typeparam>
    public class TransitionNotFoundEventArgs<TState> : EventArgs
    {
        public TState From { get; private set; }
        public IEvent Event { get; private set; }

        internal TransitionNotFoundEventArgs(TState from, IEvent evt)
        {
            this.From = from;
            this.Event = evt;
        }
    }
}
