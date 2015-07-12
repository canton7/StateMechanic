using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    /// <summary>
    /// Event args containing information on how a state machine faulteds
    /// </summary>
    public class StateMachineFaultedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets information on which component faulted, and the exception which caused that fault
        /// </summary>
        public StateMachineFaultInfo FaultInfo { get; private set; }

        internal StateMachineFaultedEventArgs(StateMachineFaultInfo faultInfo)
        {
            this.FaultInfo = faultInfo;
        }
    }
}
