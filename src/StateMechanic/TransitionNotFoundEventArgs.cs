using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public class TransitionNotFoundEventArgs<TState> : EventArgs
    {
        public TState From { get; private set; }
        public IEvent Event { get; private set; }

        public TransitionNotFoundEventArgs(TState from, IEvent evt)
        {
            this.From = from;
            this.Event = evt;
        }
    }
}
