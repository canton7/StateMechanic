using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    /// <summary>
    /// Event args containing information on a transition which executed
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    public class TransitionEventArgs<TState> : EventArgs
    {
        /// <summary>
        /// Gets the state this transition wsas from
        /// </summary>
        public TState From { get; private set; }

        /// <summary>
        /// Gets the state this transition was to
        /// </summary>
        public TState To { get; private set; }

        /// <summary>
        /// Gets the event which triggered this transition
        /// </summary>
        public IEvent Event { get; private set; }

        /// <summary>
        /// Gets the state machine on which the transition occurred
        /// </summary>
        public IStateMachine StateMachine { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this is an inner self transition, i.e. whether entry/exit handler are not executed
        /// </summary>
        public bool IsInnerTransition { get; private set; }

        internal TransitionEventArgs(TState from, TState to, IEvent evt, IStateMachine stateMachine, bool isInnerTransition)
        {
            this.From = from;
            this.To = to;
            this.Event = evt;
            this.StateMachine = stateMachine;
            this.IsInnerTransition = isInnerTransition;
        }
    }
}
