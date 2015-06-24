using StateMechanic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            var sm = new StateMachine("testy");

            var state1 = sm.CreateInitialState("State 1").WithEntry(i => Debug.WriteLine("State 1 Entry")).WithExit(i => Debug.WriteLine("State 1 Exit"));
            var state2 = sm.CreateState("State 2").WithEntry(i => Debug.WriteLine("State 2 Entry")).WithExit(i => Debug.WriteLine("State 2 Exit"));

            var event1 = sm.CreateEvent("Event 1");
            var event2 = sm.CreateEvent("Event 2");

            state1.AddTransitionOn(event1).To(state2);
            state2.AddTransitionOn(event2).To(state1);

            var subSm = state2.CreateChildStateMachine("childSm");

            var state11 = subSm.CreateInitialState("State 1.1").WithEntry(i => Debug.WriteLine("State 1.1 Entry")).WithExit(i => Debug.WriteLine("State 1.1 Exit"));
            var state12 = subSm.CreateState("State 1.2").WithEntry(i => Debug.WriteLine("State 1.2 Entry")).WithExit(i => Debug.WriteLine("State 1.2 Exit"));
            //state11.AddTransitionOn(event1).To(state12);


            event1.Fire();
            event1.Fire();
            event1.Fire();
            event2.Fire();
        }
    }
}
