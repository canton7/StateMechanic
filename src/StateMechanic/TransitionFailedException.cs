using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public class TransitionFailedException : Exception
    {
        public StateMachineFaultInfo FaultInfo { get; private set; }

        public TransitionFailedException(StateMachineFaultInfo faultInfo)
            : base(String.Format("The transition from {0} to {1} (on event {2}) failed at stage {3} with exception '{4}'",
            faultInfo.FromState.Name, faultInfo.ToState.Name, faultInfo.Event.Name, faultInfo.FaultedComponent, faultInfo.Exception.Message), faultInfo.Exception)
        {
            this.FaultInfo = faultInfo;
        }
    }
}
