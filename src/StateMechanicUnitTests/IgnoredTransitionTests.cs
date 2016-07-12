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
    public class IgnoredTransitionTests
    { 
        [Test]
        public void IgnoredEventsDontTakePriorityOverNormalTransitions()
        {
            var sm = new StateMachine("sm");
            var state1 = sm.CreateInitialState("state1");
            var state2 = sm.CreateState("state2");
            var evt = new Event("evt");
            var evt2 = new Event("evt2");

            state1.TransitionOn(evt).To(state2).WithGuard(i => true);
            state1.Ignore(evt, evt2);

            evt.Fire();

            Assert.True(state2.IsCurrent);
        }

        [Test]
        public void IgnoredEventsDontTriggerTransitionEvents()
        {
            var sm = new StateMachine("sm");
            var state1 = sm.CreateInitialState("state1");
            var evt = new Event("evt");

            state1.Ignore(evt);

            bool called = false;
            sm.Transition += (o, e) => called = true;

            evt.Fire();

            Assert.False(called);
        }

        [Test]
        public void IgnoredTEventDontTriggerTransitionNotFoundEvents()
        {
            var sm = new StateMachine("sm");
            var state1 = sm.CreateInitialState("state1");
            var evt = new Event<string>("evt");

            state1.Ignore(evt);

            bool called = false;
            sm.TransitionNotFound += (o, e) => called = true;

            evt.Fire("foo");

            Assert.False(called);
        }

        [Test]
        public void IgnoredEventsDoTriggerEventIgnoredEvents()
        {
            var sm = new StateMachine("sm");
            var state1 = sm.CreateInitialState("state1");
            var evt = new Event<string>("evt");
            var evt2 = new Event<string>("evt2");

            state1.Ignore(evt, evt2);

            EventIgnoredEventArgs<State> ea = null;
            sm.EventIgnored += (o, e) => ea = e;

            evt.Fire("foo");

            Assert.NotNull(ea);
            Assert.AreEqual(evt, ea.Event);
            Assert.AreEqual(state1, ea.State);
            Assert.AreEqual(EventFireMethod.Fire, ea.EventFireMethod);
        }

        [Test]
        public void IgnoredTryFireReturnsTrue()
        {
            var sm = new StateMachine("sm");
            var state1 = sm.CreateInitialState("state1");
            var evt = new Event("evt");

            state1.Ignore(evt);

            Assert.True(evt.TryFire());
        }
    }
}
