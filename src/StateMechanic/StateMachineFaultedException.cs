using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public class StateMachineFaultedException : Exception
    {
        public StateMachineFaultInfo FaultInfo { get; private set; }

        public StateMachineFaultedException(StateMachineFaultInfo faultInfo)
            : base(String.Format("The state machine {0} is currently faulted because a previous transition threw an exception. See FaultInfo for details", faultInfo.StateMachine.Name))
        {
            this.FaultInfo = faultInfo;
        }
    }
}
