using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public class TransitionEventArgs<TState> : EventArgs
    {
        public TState From { get; private set; }
        public TState To { get; private set; }
        public IEvent Event { get; private set; }
        public bool IsInnerTransition { get; private set; }

        public TransitionEventArgs(TState from, TState to, IEvent evt, bool isInnerTransition)
        {
            this.From = from;
            this.To = to;
            this.Event = evt;
            this.IsInnerTransition = isInnerTransition;
        }
    }
}
