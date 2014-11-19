using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public class StateHandlerInfo<TState>
    {
        public TState From { get; private set; }
        public TState To { get; private set; }
        public IEvent Event { get; private set; }

        public StateHandlerInfo(TState from, TState to, IEvent evt)
        {
            this.From = from;
            this.To = to;
            this.Event = evt;
        }
    }
}
