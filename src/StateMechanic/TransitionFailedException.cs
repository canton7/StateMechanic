using System;

namespace StateMechanic
{
    /// <summary>
    /// Indicates that a transition failed, because some part of it threw an exception
    /// </summary>
    public class TransitionFailedException : Exception
    {
        /// <summary>
        /// Gets more information on what faulted, an how
        /// </summary>
        public StateMachineFaultInfo FaultInfo { get; private set; }

        internal TransitionFailedException(StateMachineFaultInfo faultInfo)
            : base(String.Format("The transition from {0} to {1} (on event {2}) failed at stage {3} with exception '{4}'",
            faultInfo.From.Name, faultInfo.To.Name, faultInfo.Event.Name, faultInfo.FaultedComponent, faultInfo.Exception.Message), faultInfo.Exception)
        {
            this.FaultInfo = faultInfo;
        }
    }
}
