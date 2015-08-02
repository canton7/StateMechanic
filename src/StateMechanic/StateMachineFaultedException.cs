using System;

namespace StateMechanic
{
    /// <summary>
    /// Exception thrown when the state machine faults, because some part threw an exception
    /// </summary>
    public class StateMachineFaultedException : Exception
    {
        /// <summary>
        /// Gets information on which component faulted, and the exception which caused that fault
        /// </summary>
        public StateMachineFaultInfo FaultInfo { get; private set; }

        internal StateMachineFaultedException(StateMachineFaultInfo faultInfo)
            : base(String.Format("The state machine {0} is currently faulted because a previous transition threw an exception. See FaultInfo for details", faultInfo.StateMachine.Name))
        {
            this.FaultInfo = faultInfo;
        }
    }
}
