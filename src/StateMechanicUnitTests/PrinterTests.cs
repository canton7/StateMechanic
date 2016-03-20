using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using StateMechanic;

namespace StateMechanicUnitTests
{
    // These tests will never fail, but they generate dot which can be checked visually

    [TestFixture]
    public class PrinterTests
    {
        [Test]
        public void FormatsStateMachinesWithNoNames()
        {
            var sm = new StateMachine();
            var state1 = sm.CreateInitialState();
            var state2 = sm.CreateState();

            var child1 = state1.CreateChildStateMachine();
            var state11 = child1.CreateInitialState();
            var state12 = child1.CreateState();

            var child2 = state2.CreateChildStateMachine();
            var state21 = child2.CreateInitialState();
            var state22 = child2.CreateState();

            var evt1 = new Event();
            var evt2 = new Event<string>();

            state1.InnerSelfTransitionOn(evt1);
            state1.TransitionOn(evt1).To(state2);

            state11.TransitionOn(evt2).ToDynamic(_ => state12);
            state12.TransitionOn(evt1).To(state11);

            state21.TransitionOn(evt1).To(state22);

            var output = sm.FormatDot();
        }
    }
}
