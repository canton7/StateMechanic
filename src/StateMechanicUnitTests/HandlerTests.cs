//using NUnit.Framework;
//using StateMechanic;
//using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using StateMechanic;

namespace StateMechanicUnitTests
{
    [TestFixture]
    public class HandlerTests
    {
        private StateMachine sm;
        private State state1;
        private State state2;
        private State state3;

        private Event event1;
        private Event event2;
        private Event event3;

        private List<string> events;

        [SetUp]
        public void SetUp()
        {
            this.events = new List<string>();

            this.sm = new StateMachine("test");
            this.state1 = this.sm.CreateInitialState("State 1").WithEntry(i => this.events.Add("State 1 Entry")).WithExit(i => this.events.Add("State 1 Exit"));
            this.state2 = this.sm.CreateState("State 2").WithEntry(i => this.events.Add("State 2 Entry")).WithExit(i => this.events.Add("State 2 Exit"));

            this.event1 = this.sm.CreateEvent("Event 1");
            this.event2 = this.sm.CreateEvent("Event 2");
            this.event3 = this.sm.CreateEvent("Event 3");

            this.state1.AddTransitionOn(this.event1).To(this.state2).WithHandler(x => this.events.Add("Transition 1 2"));
            this.state1.AddTransitionOn(this.event2).To(this.state1).WithHandler(i => this.events.Add("Transition 1 1"));
            this.state1.AddInnerSelfTransitionOn(this.event3).WithHandler(i => this.events.Add("Transition 1 1 Inner"));
        }

        [Test]
        public void CorrectHandlersAreInvokedInNormalTransition()
        {
            this.event1.Fire();

            Assert.That(this.events, Is.EquivalentTo(new[] { "State 1 Exit", "Transition 1 2", "State 2 Entry" }));
        }

        [Test]
        public void NormalSelfTransitionShouldFireExitAndEntry()
        {
            this.event2.Fire();

            Assert.That(this.events, Is.EquivalentTo(new[] { "State 1 Exit", "Transition 1 1", "State 1 Entry" }));
        }

        [Test]
        public void InnerSelfTransitionShouldNotFireExitAndEntry()
        {
            this.event3.Fire();

            Assert.That(this.events, Is.EquivalentTo(new[] { "Transition 1 1 Inner" }));
        }
    }
}
