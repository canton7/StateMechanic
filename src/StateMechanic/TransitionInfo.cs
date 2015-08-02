using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    /// <summary>
    /// Contains information on the currently-executing transition
    /// </summary>
    /// <typeparam name="TState">Type of state</typeparam>
    public class TransitionInfo<TState>
    {
        /// <summary>
        /// Gets the state this transition is from
        /// </summary>
        public TState From { get; private set; }

        /// <summary>
        /// Gets the state this transition is to
        /// </summary>
        public TState To { get; private set; }

        /// <summary>
        /// Gets the event which triggered this transition
        /// </summary>
        public Event Event { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this is an inner self transition, i.e. whether entry/exit handler are not executed
        /// </summary>
        public bool IsInnerTransition { get; private set; }

        internal TransitionInfo(TState from, TState to, Event @event, bool isInnerTransition)
        {
            this.From = from;
            this.To = to;
            this.Event = @event;
            this.IsInnerTransition = isInnerTransition;
        }
    }

    /// <summary>
    /// Contains information on the currently-executing transition
    /// </summary>
    /// <typeparam name="TState">Type of state</typeparam>
    /// <typeparam name="TEventData">Type of event data associated with the event</typeparam>
    public class TransitionInfo<TState, TEventData>
    {
        /// <summary>
        /// Gets the state this transition is from
        /// </summary>
        public TState From { get; private set; }

        /// <summary>
        /// Gets the state this transition is to
        /// </summary>
        public TState To { get; private set; }

        /// <summary>
        /// Gets the event which triggered this transition
        /// </summary>
        public Event<TEventData> Event { get; private set; }

        /// <summary>
        /// Gets the event data which was passed when the event was fired
        /// </summary>
        public TEventData EventData { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this is an inner self transition, i.e. whether entry/exit handler are not executed
        /// </summary>
        public bool IsInnerTransition { get; private set; }

        internal TransitionInfo(TState from, TState to, Event<TEventData> @event, TEventData eventData, bool isInnerTransition)
        {
            this.From = from;
            this.To = to;
            this.Event = @event;
            this.EventData = eventData;
            this.IsInnerTransition = isInnerTransition;
        }
    }
}
