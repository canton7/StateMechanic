using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    /// <summary>
    /// Passed from an event to the state machine, this contains the information required to fire an event
    /// </summary>
    internal struct EventFireData
    {
        public IEventInternal SourceEvent { get; }
        public EventFireMethod EventFireMethod { get; }
        // Having this as 'object' is ugly, but the alternative is a new delegate creation on each event invocation, which is more expensive
        public object TransitionData { get; }

        public EventFireData(IEventInternal sourceEvent, EventFireMethod eventFireMethod, object transitionData)
        {
            this.SourceEvent = sourceEvent;
            this.EventFireMethod = eventFireMethod;
            this.TransitionData = transitionData;
        }
    }
}
