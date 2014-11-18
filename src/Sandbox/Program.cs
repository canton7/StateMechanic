using StateMechanic;
using System;
using System.Collections.Generic;
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
            var state1 = sm.CreateState("state1");
            var state2 = sm.CreateState("state2");

            var event1 = sm.CreateEvent<object>("event1");
        }
    }
}
