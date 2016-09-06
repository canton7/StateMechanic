using System;

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
