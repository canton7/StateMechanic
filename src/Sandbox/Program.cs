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
            var state1 = sm.CreateInitialState("state1");
            var state2 = sm.CreateState("state2");

            var event1 = sm.CreateEvent<string>("event1");

            state1.AddTransitionOn(event1).To(state2).WithHandler(info => Debug.WriteLine(info));

            event1.Fire("hello");
        }
    }
}
