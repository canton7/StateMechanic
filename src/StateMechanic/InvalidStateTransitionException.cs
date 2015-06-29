using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public class InvalidStateTransitionException : Exception
    {
        public IState From { get; private set; }
        public IState To { get; private set; }

        internal InvalidStateTransitionException(IState from, IState to)
            : base(String.Format("Unable to transition from {0} to {1}, as they belong to different state machines", from.Name, to.Name))
        {
            this.From = from;
            this.To = to;
        }
    }
}
