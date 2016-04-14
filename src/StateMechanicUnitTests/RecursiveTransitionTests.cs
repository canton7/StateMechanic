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
    public class RecursiveTransitionTests
    {
        [Test]
        public void EventFireInTransitionHandlerIsQueued()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state1");
            var state2 = sm.CreateState("state2");
            var state3 = sm.CreateState("state3");

            var evt = new Event("evt");
            var evt2 = new Event("evt2");

            initial.TransitionOn(evt).To(state1).WithHandler(i => evt2.Fire());
            initial.TransitionOn(evt2).To(state2);
            state1.TransitionOn(evt2).To(state3);

            evt.Fire();

            Assert.AreEqual(state3, sm.CurrentState);
        }

        [Test]
        public void ForceTransitionIsQueued()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state1");
            var state2 = sm.CreateState("state2");
            var evt = new Event("evt");

            State entryFrom = null;
            state2.EntryHandler = i => entryFrom = i.From;

            initial.TransitionOn(evt).To(state1).WithHandler(i => sm.ForceTransition(state2, evt));

            evt.Fire();

            Assert.AreEqual(state1, entryFrom);
        }

        [Test]
        public void TransitionFromGuardIsCorrectlyQueuedIfGuardReturnsFalse()
        {
            var log = new List<string>();

            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state1").WithEntry(_ => log.Add("state1 entered"));
            var state2 = sm.CreateState("state2").WithEntry(_ => log.Add("state2 entered"));

            var event1 = new Event("event1");
            var event2 = new Event("event2");

            initial.TransitionOn(event1).To(state1).WithGuard(_ =>
            {
                event2.Fire();
                log.Add("event2 fired");
                return false;
            });

            initial.TransitionOn(event2).To(state2);

            event1.TryFire();

            Assert.That(log, Is.EquivalentTo(new[] { "event2 fired", "state2 entered" }));
            Assert.AreEqual(state2, sm.CurrentState);
        }

        [Test]
        public void TransitionFromGuardIsCorrectlyQueuedIfGuardReturnsTrue()
        {
            var log = new List<string>();

            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state1").WithEntry(_ => log.Add("state1 entered"));
            var state2 = sm.CreateState("state2").WithEntry(_ => log.Add("state2 entered"));

            var event1 = new Event("event1");
            var event2 = new Event("event2");

            initial.TransitionOn(event1).To(state1).WithGuard(_ =>
            {
                event2.Fire();
                log.Add("event2 fired");
                return true;
            });

            state1.TransitionOn(event2).To(state2);

            event1.TryFire();

            Assert.That(log, Is.EquivalentTo(new[] { "event2 fired", "state1 entered", "state2 entered" }));
            Assert.AreEqual(state2, sm.CurrentState);
        }
    }
}
