using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    /// <summary>
    /// Indicates which component of the transition threw an exception
    /// </summary>
    public enum FaultedComponent
    {
        /// <summary>
        /// A transition guard threw an exception
        /// </summary>
        Guard,

        /// <summary>
        /// A state exit handler threw an excpetion
        /// </summary>
        ExitHandler,

        /// <summary>
        /// A transition handler threw an exception
        /// </summary>
        TransitionHandler,

        /// <summary>
        /// A state entry handler threw an exception
        /// </summary>
        EntryHandler
    }

    public class StateMachineFaultInfo
    {
        public IStateMachine StateMachine { get; private set; }
        public FaultedComponent FaultedComponent { get; private set; }
        public Exception Exception { get; private set; }
        public IState From { get; private set; }
        public IState To { get; private set; }
        public IEvent Event { get; private set; }

        public StateMachineFaultInfo(IStateMachine stateMachine, FaultedComponent faultedComponent, Exception exception, IState from, IState to, IEvent evt)
        {
            this.StateMachine = stateMachine;
            this.FaultedComponent = faultedComponent;
            this.Exception = exception;
            this.From = from;
            this.To = to;
            this.Event = evt;
        }
    }
}
