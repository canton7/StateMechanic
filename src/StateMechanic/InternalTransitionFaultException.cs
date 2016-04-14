using System;

namespace StateMechanic
{
    internal class InternalTransitionFaultException : Exception
    {
        public IState From { get; }
        public IState To { get; }
        public IEvent Event { get; }
        public FaultedComponent FaultedComponent { get; }
        public IStateGroup Group { get; }

        public InternalTransitionFaultException(IState from, IState to, IEvent @event, FaultedComponent faultedComponent, Exception innerException, IStateGroup group = null)
            : base("A transition threw an exception", innerException)
        {
            this.From = from;
            this.To = to;
            this.Event = @event;
            this.FaultedComponent = faultedComponent;
            this.Group = group;
        }
    }
}
