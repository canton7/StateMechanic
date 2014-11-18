using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public class TransitionInfo<TState>
    {
        public TState From { get; private set; }
        public TState To { get; private set; }
        public Event Event { get; private set; }

        public TransitionInfo(TState from, TState to, Event evt)
        {
            this.From = from;
            this.To = to;
            this.Event = evt;
        }
    }

    public class TransitionInfo<TState, TEventData>
    {
        public TState From { get; private set; }
        public TState To { get; private set; }
        public Event<TEventData> Event { get; private set; }
        public TEventData EventData { get; private set; }

        public TransitionInfo(TState from, TState to, Event<TEventData> evt, TEventData eventData)
        {
            this.From = from;
            this.To = to;
            this.Event = evt;
            this.EventData = eventData;
        }
    }
}
