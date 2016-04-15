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
    public class EventFireTests
    {
        [Test]
        public void FireThrowsIfNotYetAssociatedWithAStateMachine()
        {
            var evt = new Event("evt");

            var e = Assert.Throws<TransitionNotFoundException>(() => evt.Fire());
            Assert.IsNull(e.StateMachine);
            Assert.IsNull(e.From);
            Assert.AreEqual(evt, e.Event);
        }

        [Test]
        public void TryFireReturnsFalseIfNotYetAssociatedStateMachine()
        {
            var evt = new Event("evt");
            Assert.False(evt.TryFire());
        }

        [Test]
        public void FireWithEventDataThrowsIfNotYetAssociatedWithAStateMachine()
        {
            var evt = new Event<string>("evt");

            var e = Assert.Throws<TransitionNotFoundException>(() => evt.Fire("boo"));
            Assert.IsNull(e.StateMachine);
            Assert.IsNull(e.From);
            Assert.AreEqual(evt, e.Event);
        }

        [Test]
        public void TryFireWithEventDataReturnsFalseIfNotYetAssociatedStateMachine()
        {
            var evt = new Event<string>("evt");
            Assert.False(evt.TryFire("foo"));
        }

        [Test]
        public void FireThrowsIfTransitionNotFound()
        {
            var sm = new StateMachine("sm");
            var initialState = sm.CreateInitialState("initialState");
            var state1 = sm.CreateState("state1");
            var evt = new Event("evt");

            state1.TransitionOn(evt).To(initialState);

            var e = Assert.Throws<TransitionNotFoundException>(() => evt.Fire());
            Assert.AreEqual(initialState, e.From);
            Assert.AreEqual(evt, e.Event);
            Assert.AreEqual(sm, e.StateMachine);
        }

        [Test]
        public void TryFireReturnsFalseIfTransitionNotFound()
        {
            var sm = new StateMachine("sm");
            var initialState = sm.CreateInitialState("initialState");
            var state1 = sm.CreateState("state1");
            var evt = new Event("evt");
            state1.InnerSelfTransitionOn(evt);

            Assert.False(evt.TryFire());
        }
    }
}
