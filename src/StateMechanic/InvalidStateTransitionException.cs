using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    /// <summary>
    /// Exception thrown when a transition is defined which is invalid
    /// </summary>
    public class InvalidStateTransitionException : Exception
    {
        /// <summary>
        /// Gets the state the transition is from
        /// </summary>
        public IState From { get; private set; }

        /// <summary>
        /// Gets the state the transition is to
        /// </summary>
        public IState To { get; private set; }

        internal InvalidStateTransitionException(IState from, IState to)
            : base(String.Format("Unable to transition from {0} to {1}, as they belong to different state machines", from.Name, to.Name))
        {
            this.From = from;
            this.To = to;
        }
    }
}
