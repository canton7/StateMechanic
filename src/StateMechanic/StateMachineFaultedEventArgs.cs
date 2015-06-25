using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public class StateMachineFaultedEventArgs : EventArgs
    {
        public StateMachineFaultInfo FaultInfo { get; private set; }

        public StateMachineFaultedEventArgs(StateMachineFaultInfo faultInfo)
        {
            this.FaultInfo = faultInfo;
        }
    }
}
