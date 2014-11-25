using NUnit.Framework;
using StateMechanic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private Event event4;

        private ITransition<State> transition12;

        private List<string> events;

        [SetUp]
        public void SetUp()
        {
            this.events = new List<string>();

            this.sm = new StateMachine("test");
            this.state1 = this.sm.CreateInitialState("State 1").WithEntry(i => this.events.Add("State 1 Entry")).WithExit(i => this.events.Add("State 1 Exit"));
            this.state2 = this.sm.CreateState("State 2").WithEntry(i => this.events.Add("State 2 Entry")).WithExit(i => this.events.Add("State 2 Exit"));
            this.state3 = this.sm.CreateState("State 3").WithEntry(i => this.events.Add("State 3 Entry")).WithExit(i => this.events.Add("State 3 Exit"));

            this.event1 = this.sm.CreateEvent("Event 1");
            this.event2 = this.sm.CreateEvent("Event 2");
            this.event3 = this.sm.CreateEvent("Event 3");
            this.event4 = this.sm.CreateEvent("Event 4");

            this.transition12 = this.state1.AddTransitionOn(this.event1).To(this.state2).WithHandler(x => this.events.Add("Transition 1 2"));
            this.state1.AddTransitionOn(this.event2).To(this.state3).WithHandler(x => this.events.Add("Transition 1 3"));

            this.state2.AddTransitionOn(this.event2).To(this.state1).WithHandler(x => this.events.Add("Transition 2 1"));

            this.state1.AddTransitionOn(this.event3).To(this.state1).WithHandler(i => this.events.Add("Transition 1 1"));
            this.state1.AddInnerSelfTransitionOn(this.event4).WithHandler(i => this.events.Add("Transition 1 1 Inner"));
        }

        [Test]
        public void CorrectHandlersAreInvokedInNormalTransition()
        {
            this.event1.Fire();

            Assert.That(this.events, Is.EquivalentTo(new[] { "State 1 Exit", "Transition 1 2", "State 2 Entry" }));
        }

        [Test]
        public void WhenEventRedirectedInExitHandlerTransitionAndNewEntryOnlyShouldBeInvoked()
        {
            this.state1.OnExit = t => { this.events.Add("State 1 Exit"); this.event2.Fire(); };

            this.event1.Fire();

            Assert.That(this.events, Is.EquivalentTo(new[] { "State 1 Exit", "Transition 1 3", "State 3 Entry" }));
        }

        [Test]
        public void WhenEventRedirectionInTransitionNewTransitionAndEntryShouldBeInvoked()
        {
            this.transition12.Handler = t => { this.events.Add("Transition 1 2"); this.event2.Fire(); };

            this.event1.Fire();

            Assert.That(this.events, Is.EquivalentTo(new[] { "State 1 Exit", "Transition 1 2", "Transition 1 3", "State 3 Entry" }));
        }

        [Test]
        public void WhenEventRedirectionInEntryHandlerExitHandlerNewTransitionAndEntryShouldBeInvoked()
        {
            this.state2.OnEntry = t => { this.events.Add("State 2 Entry"); this.event2.Fire(); };

            this.event1.Fire();

            Assert.That(this.events, Is.EquivalentTo(new[] { "State 1 Exit", "Transition 1 2", "State 2 Entry", "State 2 Exit", "Transition 2 1", "State 1 Entry" }));
        }

        [Test]
        public void NormalSelfTransitionShouldFireExitAndEntry()
        {
            this.event3.Fire();

            Assert.That(this.events, Is.EquivalentTo(new[] { "State 1 Exit", "Transition 1 1", "State 1 Entry" }));
        }

        [Test]
        public void InnerSelfTransitionShouldNotFireExitAndEntry()
        {
            this.event4.Fire();

            Assert.That(this.events, Is.EquivalentTo(new[] { "Transition 1 1 Inner" }));
        }
    }
}
