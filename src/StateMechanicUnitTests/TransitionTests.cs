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
    public class TransitionTests
    {
        [Test]
        public void FirstRegisteredTransitionWins()
        {
            var sm = new StateMachine("state machine");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state1");
            var state2 = sm.CreateState("state2");

            var evt = sm.CreateEvent("evt");

            initial.TransitionOn(evt).To(state2);
            initial.TransitionOn(evt).To(state1);

            evt.Fire();

            Assert.AreEqual(state2, sm.CurrentState);
        }

        [Test]
        public void FirstRegisteredTransitionWithTrueGuardWins()
        {
            var sm = new StateMachine("state machine");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state1");
            var state2 = sm.CreateState("state2");
            var state3 = sm.CreateState("state3");
            var state4 = sm.CreateState("state4");

            var evt = sm.CreateEvent("evt");

            initial.TransitionOn(evt).To(state1).WithGuard(i => false);
            initial.TransitionOn(evt).To(state2).WithGuard(i => false);
            initial.TransitionOn(evt).To(state3);
            initial.TransitionOn(evt).To(state4);

            evt.Fire();

            Assert.AreEqual(state3, sm.CurrentState);
        }

        [Test]
        public void TransitionIsAbortedIfAnyGuardThrowsAnException()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state1");
            var evt = sm.CreateEvent("evt");

            var exception = new Exception("foo");
            initial.TransitionOn(evt).To(initial).WithGuard(i => { throw exception; });
            initial.TransitionOn(evt).To(state1);

            var e = Assert.Throws<Exception>(() => evt.Fire());
            Assert.AreEqual(exception, e);
            Assert.AreEqual(initial, sm.CurrentState);
        }

        [Test]
        public void EventFireInTransitionHandlerIsQueued()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state1");
            var state2 = sm.CreateState("state2");
            var state3 = sm.CreateState("state3");

            var evt = sm.CreateEvent("evt");
            var evt2 = sm.CreateEvent("evt2");

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
            var evt = sm.CreateEvent("evt");

            State entryFrom = null;
            state2.EntryHandler = i => entryFrom = i.From;

            initial.TransitionOn(evt).To(state1).WithHandler(i => sm.ForceTransition(state2, evt));

            evt.Fire();

            Assert.AreEqual(state1, entryFrom);
        }

        [Test]
        public void ParentDoesNotTransitionIfChildTransitions()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state1");

            var child = initial.CreateChildStateMachine();
            var childInitial = child.CreateInitialState("childInitial");
            var childState1 = child.CreateState("childState1");

            var evt = sm.CreateEvent("evt");
            initial.TransitionOn(evt).To(state1);
            childInitial.TransitionOn(evt).To(childState1);

            evt.Fire();

            Assert.AreEqual(childState1, child.CurrentState);
            Assert.AreEqual(initial, sm.CurrentState);
        }
    }
}
