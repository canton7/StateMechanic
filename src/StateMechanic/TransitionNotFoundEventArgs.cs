using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public class TransitionNotFoundEventArgs<TState> : EventArgs
    {
        public IStateMachine StateMachine { get; private set; }
        public TState From { get; private set; }
        public IEvent Event { get; private set; }

        public TransitionNotFoundEventArgs(IStateMachine stateMachine, TState from, IEvent evt)
        {
            this.StateMachine = stateMachine;
            this.From = from;
            this.Event = evt;
        }
    }
}
