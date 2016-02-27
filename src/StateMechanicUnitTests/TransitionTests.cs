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

            var evt = new Event("evt");

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

            var evt = new Event("evt");

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
            var evt = new Event("evt");

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
        public void ParentDoesNotTransitionIfChildTransitions()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state1");

            var child = initial.CreateChildStateMachine();
            var childInitial = child.CreateInitialState("childInitial");
            var childState1 = child.CreateState("childState1");

            var evt = new Event("evt");
            initial.TransitionOn(evt).To(state1);
            childInitial.TransitionOn(evt).To(childState1);

            evt.Fire();

            Assert.AreEqual(childState1, child.CurrentState);
            Assert.AreEqual(initial, sm.CurrentState);
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

        [Test]
        public void StateSelectorGetsCorrectInfo()
        {
            DynamicSelectorInfo<State> info = null;

            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var evt = new Event("evt");
            initial.TransitionOn(evt).ToDynamic(i =>
            {
                info = i;
                return initial;
            });

            evt.Fire();

            Assert.NotNull(info);
            Assert.AreEqual(initial, info.From);
            Assert.AreEqual(evt, info.Event);
        }

        [Test]
        public void StateSelectorWithEventDataGetsCorrectInfo()
        {
            DynamicSelectorInfo<State, string> info = null;

            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var evt = new Event<string>("evt");
            initial.TransitionOn(evt).ToDynamic(i =>
            {
                info = i;
                return initial;
            });

            evt.Fire("hello");

            Assert.NotNull(info);
            Assert.AreEqual(initial, info.From);
            Assert.AreEqual(evt, info.Event);
            Assert.AreEqual("hello", info.EventData);
        }

        [Test]
        public void DynamicTransitionHandlerGetsCorrectInfo()
        {
            TransitionInfo<State> info = null;

            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state1");
            var evt = new Event("evt");
            initial.TransitionOn(evt).ToDynamic(i => state1).WithHandler(i => info = i);

            evt.Fire();

            Assert.NotNull(info);
            Assert.AreEqual(initial, info.From);
            Assert.AreEqual(state1, info.To);
            Assert.AreEqual(evt, info.Event);
            Assert.False(info.IsInnerTransition);
        }

        [Test]
        public void DynamicTransitionHandlerWithEventDataGetsCorrectInfo()
        {
            TransitionInfo<State, string> info = null;

            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state1");
            var evt = new Event<string>("evt");
            initial.TransitionOn(evt).ToDynamic(i => state1).WithHandler(i => info = i);

            evt.Fire("foo");

            Assert.NotNull(info);
            Assert.AreEqual(initial, info.From);
            Assert.AreEqual(state1, info.To);
            Assert.AreEqual(evt, info.Event);
            Assert.False(info.IsInnerTransition);
            Assert.AreEqual("foo", info.EventData);
        }
    }
}
