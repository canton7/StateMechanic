using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public class TransitionInfo<TState, TEvent>
    {
        public TState From { get; private set; }
        public TState To { get; private set; }
        public TEvent Event { get; private set; }

        public TransitionInfo(TState from, TState to, TEvent evt)
        {
            this.From = from;
            this.To = to;
            this.Event = evt;
        }
    }
}
