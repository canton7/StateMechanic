using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using StateMechanic;

namespace StateMechanicUnitTests
{
    [TestFixture]
    public class TransitionNotificationTests
    {
        private StateMachine stateMachine;

        private State state1;
        private State state2;

        private Event event1;
        private Event event2;
        private Event event3;

        [SetUp]
        public void SetUp()
        {
            this.stateMachine = new StateMachine("State Machine");

            this.state1 = this.stateMachine.CreateInitialState("State 1");
            this.state2 = this.stateMachine.CreateState("State 2");

            this.event1 = this.stateMachine.CreateEvent("Event 1");
            this.event2 = this.stateMachine.CreateEvent("Event 2");
            this.event3 = this.stateMachine.CreateEvent("Event 3");

            this.state1.TransitionOn(this.event1).To(this.state2);
            this.state1.InnerSelfTransitionOn(this.event2);
        }

        [Test]
        public void NotifiesOfTransition()
        {
            TransitionEventArgs<State> ea = null;
            this.stateMachine.Transition += (o, e) =>
            {
                ea = e;
            };

            this.event1.Fire();

            Assert.NotNull(ea);
            Assert.AreEqual(this.state1, ea.From);
            Assert.AreEqual(this.state2, ea.To);
            Assert.AreEqual(this.event1, ea.Event);
            Assert.False(ea.IsInnerTransition);
        }

        [Test]
        public void NotifiesOfInnerSelfTransition()
        {
            TransitionEventArgs<State> ea = null;
            this.stateMachine.Transition += (o, e) =>
            {
                ea = e;
            };

            this.event2.Fire();

            Assert.NotNull(ea);
            Assert.AreEqual(this.state1, ea.From);
            Assert.AreEqual(this.state1, ea.To);
            Assert.AreEqual(this.event2, ea.Event);
            Assert.True(ea.IsInnerTransition);
        }

        [Test]
        public void NotifiesOfTransitionNotFound()
        {
            TransitionNotFoundEventArgs<State> ea = null;
            this.stateMachine.TransitionNotFound += (o, e) =>
            {
                ea = e;
            };

            this.event3.TryFire();

            Assert.NotNull(ea);
            Assert.AreEqual(this.event3, ea.Event);
            Assert.AreEqual(this.state1, ea.From);
        }
    }
}
