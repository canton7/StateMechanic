using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    internal class InternalTransitionFaultException : Exception
    {
        public IState From { get; private set; }
        public IState To { get; private set; }
        public IEvent Event { get; private set; }
        public FaultedComponent FaultedComponent { get; private set; }

        public InternalTransitionFaultException(IState from, IState to, IEvent @event, FaultedComponent faultedComponent, Exception innerException)
            : base("A transition threw an exception", innerException)
        {
            this.From = from;
            this.To = to;
            this.Event = @event;
            this.FaultedComponent = faultedComponent;
        }
    }
}
