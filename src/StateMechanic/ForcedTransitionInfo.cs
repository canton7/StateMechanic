using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    internal class ForcedTransitionInfo<TState> : ITransitionInfo<TState>
    {
        public TState From { get; }

        public TState To { get; }

        public IEvent Event { get; }

        public object EventData { get; }

        public bool IsInnerTransition => false;

        public EventFireMethod EventFireMethod { get; }

        public ForcedTransitionInfo(TState from, TState to, IEvent @event, object eventData, EventFireMethod eventFireMethod)
        {
            this.From = from;
            this.To = to;
            this.Event = @event;
            this.EventData = eventData;
            this.EventFireMethod = eventFireMethod;
        }
    }
}
