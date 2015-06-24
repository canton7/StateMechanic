using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public class InvalidTransitionException : Exception
    {
        public IState From { get; private set; }
        public IState To { get; private set; }

        internal InvalidTransitionException(IState from, IState to)
            : base(String.Format("Unable to create transition from {0} to {1}, as they belong to different state machines", from.Name, to.Name))
        {
            this.From = from;
            this.To = to;
        }
    }
}
