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
        public void FormatsStateMachines()
        {
            var sm = new StateMachine("sm");
            var state1 = sm.CreateInitialState("state1");
            var state2 = sm.CreateState("state2");

            var child1 = state1.CreateChildStateMachine("child1");
            var state11 = child1.CreateInitialState("state11");
            var state12 = child1.CreateState("state12");

            var child2 = state2.CreateChildStateMachine("child2");
            var state21 = child2.CreateInitialState("state21");
            var state22 = child2.CreateState("state22");

            var evt1 = new Event("evt1");
            var evt2 = new Event<string>("evt2");

            state1.InnerSelfTransitionOn(evt1);
            state1.TransitionOn(evt1).To(state2);

            state11.TransitionOn(evt2).ToDynamic(_ => state12);
            state12.TransitionOn(evt1).To(state11);

            state21.TransitionOn(evt1).To(state22);
            state21.TransitionOn(evt2).To(state22);

            var dot = sm.FormatDot(colorize: true);
            var dgml = sm.FormatDgml(colorize: true);
        }

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

            var dot = sm.FormatDot();
            var dgml = sm.FormatDgml();
        }
    }
}
