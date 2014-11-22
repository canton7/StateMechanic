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
            var state3 = sm.CreateState("state3");

            var event1 = sm.CreateEvent("event1");
            var event2 = sm.CreateEvent<object>("event1");

            state1.OnEntry = t => Debug.WriteLine("State 1 entry");
            state1.OnExit = t => { Debug.WriteLine("State 1 exit"); };

            state2.OnEntry = t => { Debug.WriteLine("State 2 entry"); };
            state2.OnExit = t => Debug.WriteLine("State 2 exit");

            state3.OnEntry = t => Debug.WriteLine("State 3 entry");
            state3.OnExit = t => Debug.WriteLine("State 3 exit");


            state1.AddTransitionOn(event1).To(state2).WithHandler(info => { Debug.WriteLine("Transition from 1 to 2 on 1"); });
            state2.AddTransitionOn(event1).To(state3).WithHandler(info => Debug.WriteLine("Transition from 2 to 3 on 2"));

            event1.Fire();
        }
    }
}
