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

        private Event event1;
        private Event event2;
        private Event event3;

        private Transition<State> transition12;

        private List<string> events;

        private struct EventData
        {
            public int Foo { get; set;  }
        }

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

            this.transition12 = this.state1.AddTransitionOn(this.event1).To(this.state2).WithHandler(x => this.events.Add("Transition 1 2"));
            this.state1.AddTransitionOn(this.event2).To(this.state1).WithHandler(i => this.events.Add("Transition 1 1"));
            this.state1.AddInnerSelfTransitionOn(this.event3).WithHandler(i => this.events.Add("Transition 1 1 Inner"));
        }

        [Test]
        public void CorrectHandlersAreInvokedInNormalTransition()
        {
            var sm = new StateMachine("State Machine");
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

        [Test]
        public void CorrectInfoIsGivenInGuard()
        {
            TransitionInfo<State> guardInfo = null;
            this.transition12.Guard = i => { guardInfo = i; return true; };

            this.event1.Fire();

            Assert.NotNull(guardInfo);
            Assert.AreEqual(this.state1, guardInfo.From);
            Assert.AreEqual(this.state2, guardInfo.To);
            Assert.AreEqual(this.event1, guardInfo.Event);
            Assert.False(guardInfo.IsInnerTransition);
        }

        [Test]
        public void CorrectInfoIsGivenInExitHandler()
        {
            StateHandlerInfo<State> handlerInfo = null;
            this.state1.ExitHandler = i => handlerInfo = i;

            this.event1.Fire();

            Assert.NotNull(handlerInfo);
            Assert.AreEqual(this.state1, handlerInfo.From);
            Assert.AreEqual(this.state2, handlerInfo.To);
            Assert.AreEqual(this.event1, handlerInfo.Event);
        }

        [Test]
        public void CorrectInfoIsGivenInEntryHandler()
        {
            StateHandlerInfo<State> handlerInfo = null;
            this.state2.EntryHandler = i => handlerInfo = i;

            this.event1.Fire();

            Assert.NotNull(handlerInfo);
            Assert.AreEqual(this.state1, handlerInfo.From);
            Assert.AreEqual(this.state2, handlerInfo.To);
            Assert.AreEqual(this.event1, handlerInfo.Event);
        }

        [Test]
        public void CorrectInfoIsGivenInTransitionHandler()
        {
            TransitionInfo<State> transitionInfo = null;
            this.transition12.Handler = i => transitionInfo = i;

            this.event1.Fire();

            Assert.NotNull(transitionInfo);
            Assert.AreEqual(this.state1, transitionInfo.From);
            Assert.AreEqual(this.state2, transitionInfo.To);
            Assert.AreEqual(this.event1, transitionInfo.Event);
            Assert.False(transitionInfo.IsInnerTransition);
        }

        [Test]
        public void EventDataIsGivenToTransitionHandler()
        {
            var evt = this.sm.CreateEvent<EventData>("Evt");
            EventData eventData = new EventData();
            this.state1.AddTransitionOn(evt).To(state2).WithHandler(i => eventData = i.EventData);

            evt.Fire(new EventData() { Foo = 2 });

            Assert.AreEqual(2, eventData.Foo);
        }
    }
}
