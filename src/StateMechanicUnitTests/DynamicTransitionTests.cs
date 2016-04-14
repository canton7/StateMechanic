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
    public class DynamicTransitionTests
    {
        [Test]
        public void StateSelectorWhichReturnsNullAbortsTransition()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var evt = new Event("evt");
            initial.TransitionOn(evt).ToDynamic(i => null);
            Assert.False(evt.TryFire());
        }

        [Test]
        public void StateSelectorChoosesNextState()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state1");
            var evt = new Event("evt");
            initial.TransitionOn(evt).ToDynamic(i => state1);

            evt.Fire();
            Assert.AreEqual(state1, sm.CurrentState);
        }

        [Test]
        public void StateSelectorForbidsTransitionToStateFromDifferentStateMachine()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var child = initial.CreateChildStateMachine();
            var childInitial = child.CreateInitialState("childInitial");
            var evt = new Event("evt");
            initial.TransitionOn(evt).ToDynamic(i => childInitial);

            var e = Assert.Throws<InvalidStateTransitionException>(() => evt.Fire());
            Assert.AreEqual(initial, e.From);
            Assert.AreEqual(childInitial, e.To);
        }

        [Test]
        public void StateSelectorCausesOuterSelfTransition()
        {
            bool exitCalled = false;
            bool entryCalled = false;

            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial")
                .WithEntry(i => entryCalled = true)
                .WithExit(i => exitCalled = true);
            var evt = new Event("evt");
            initial.TransitionOn(evt).ToDynamic(i => initial);

            evt.Fire();
            Assert.True(entryCalled);
            Assert.True(exitCalled);
        }
    }
}
