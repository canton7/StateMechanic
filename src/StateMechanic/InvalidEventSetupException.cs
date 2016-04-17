using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    /// <summary>
    /// Thrown when an Event is fired, but hasn't yet been associated with a state machine
    /// </summary>
    public class InvalidEventSetupException : Exception
    {
        internal InvalidEventSetupException(IEvent @event)
            : base($"Event {@event.Name} has not been associated with any transitions, and could not be invoked")
        {
        }
    }
}
