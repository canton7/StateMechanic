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
    public class StateSubclassTests
    {
        private class SubclassWithBadT : StateBase<State>
        {
        }

        private class SimpleSubclass : StateBase<SimpleSubclass>
        {
        }

        [Test]
        public void StateBaseThrowsIfSubclassDoesNotProvideoCorrectT()
        {
            Assert.Throws<ArgumentException>(() => new SubclassWithBadT());
        }

        [Test]
        public void ThrowsIfStateIsNotCreatedFromStateMachine()
        {
            var sm = new StateMachine<SimpleSubclass>();
            var state1 = sm.CreateInitialState("state1");
            var state2 = new SimpleSubclass();
            var evt = new Event("evt");

            Assert.Throws<InvalidOperationException>(() => state1.TransitionOn(evt).To(state2));
        }
    }
}
