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
    public class AssertionTests
    {
        [Test]
        public void ThrowsIfParentStateUsedInChildStateMachine()
        {
            var sm = new StateMachine("State Machine");
            var state = sm.CreateInitialState("Initial State");
            var subSm = state.CreateChildStateMachine();
            var subState = subSm.CreateInitialState("Child Initial State");
            var evt = new Event("Event");
            
            var e = Assert.Throws<InvalidStateTransitionException>(() => subState.TransitionOn(evt).To(state));
            Assert.AreEqual(subState, e.From);
            Assert.AreEqual(state, e.To);
        }

        [Test]
        public void ThrowsIfChildStateUsedInParentStateMachine()
        {
            var sm = new StateMachine("State Machine");
            var state = sm.CreateInitialState("Initial State");
            var subSm = state.CreateChildStateMachine();
            var subState = subSm.CreateInitialState("Child Initial State");
            var evt = new Event("Event");

            var e = Assert.Throws<InvalidStateTransitionException>(() => state.TransitionOn(evt).To(subState));
            Assert.AreEqual(state, e.From);
            Assert.AreEqual(subState, e.To);
        }

        [Test]
        public void ThrowsIfEventUsedOnTwoDifferentStateMachines()
        {
            var sm1 = new StateMachine("State Machine 1");
            var state1 = sm1.CreateInitialState("Initial State");

            var sm2 = new StateMachine("State Machine 2");
            var state2 = sm2.CreateInitialState("Initial State");

            var evt = new Event("Event");

            state1.TransitionOn(evt).To(state1);

            var e = Assert.Throws<InvalidEventTransitionException>(() => state2.TransitionOn(evt).To(state2));
            Assert.AreEqual(state2, e.From);
            Assert.AreEqual(evt, e.Event);
        }

        [Test]
        public void DoesNotThrowIfParentEventUsedInChildStateMachine()
        {
            var sm = new StateMachine("State Machine");
            var state = sm.CreateInitialState("Initial State");
            var subSm = state.CreateChildStateMachine();
            var subState = subSm.CreateInitialState("Child Initial State");
            var evt = new Event("Event");

            Assert.DoesNotThrow(() => subState.TransitionOn(evt).To(subState));
        }

        [Test]
        public void ThrowsIfEventFiredAndInitialStateNotSet()
        {
            var sm = new StateMachine("State Machine");
            var state1 = sm.CreateState("Initial State");
            var evt = new Event("Event");

            state1.InnerSelfTransitionOn(evt);

            Assert.Throws<InvalidOperationException>(() => evt.Fire());
        }

        [Test]
        public void ThrowsIfInitialStateSetTwice()
        {
            var sm = new StateMachine("State machine");
            var state1 = sm.CreateInitialState("State 1");
            Assert.Throws<InvalidOperationException>(() => sm.CreateInitialState("State 2"));
        }

        [Test]
        public void ThrowsIfTransitionOnIsNull()
        {
            var sm = new StateMachine("State machine");
            var state1 = sm.CreateInitialState("state1");
            Assert.Throws<ArgumentNullException>(() => state1.TransitionOn(null));
        }

        [Test]
        public void ThrowsIfTransitionOnWithEventDataIsNull()
        {
            var sm = new StateMachine("State machine");
            var state1 = sm.CreateInitialState("state1");
            Assert.Throws<ArgumentNullException>(() => state1.TransitionOn<string>(null));
        }

        [Test]
        public void ThrowsIfInnerSelfTransitionOnIsNull()
        {
            var sm = new StateMachine("State machine");
            var state1 = sm.CreateInitialState("state1");
            Assert.Throws<ArgumentNullException>(() => state1.InnerSelfTransitionOn(null));
        }

        [Test]
        public void ThrowsIfInnerSelfTransitionOnWithEventDataIsNull()
        {
            var sm = new StateMachine("State machine");
            var state1 = sm.CreateInitialState("state1");
            Assert.Throws<ArgumentNullException>(() => state1.InnerSelfTransitionOn<string>(null));
        }

        [Test]
        public void ThrowsIfCreateChildStateMachineCalledOnAStatWhichAlreadyHasAChildStateMachine()
        {
            var sm = new StateMachine("sm");
            var state1 = sm.CreateInitialState("state1");
            var subSm = state1.CreateChildStateMachine();
            Assert.Throws<InvalidOperationException>(() => state1.CreateChildStateMachine());
        }

        [Test]
        public void StateThrowsIfAddToGroupCalledWithNull()
        {
            var sm = new StateMachine("sm");
            var state1 = sm.CreateInitialState("state1");
            Assert.Throws<ArgumentNullException>(() => state1.AddToGroup(null));
        }

        [Test]
        public void StateThrowsIfAddToGroupsCalledWithNull()
        {
            var sm = new StateMachine("sm");
            var state1 = sm.CreateInitialState("state1");
            Assert.Throws<ArgumentNullException>(() => state1.AddToGroups(null));
        }

        [Test]
        public void StateGroupThrowsIfAddStateCalledWithNull()
        {
            var group = new StateGroup();
            Assert.Throws<ArgumentNullException>(() => group.AddState(null));
        }

        [Test]
        public void StateGroupThrowsIfAddStatesCalledWithNull()
        {
            var group = new StateGroup();
            Assert.Throws<ArgumentNullException>(() => group.AddStates(null));
        }

        [Test]
        public void IsChildOfThrowsIfArgumentIsNull()
        {
            var sm = new StateMachine();
            Assert.Throws<ArgumentNullException>(() => sm.IsChildOf(null));
        }

        [Test]
        public void StateMachineSerializerThrowsIfNullSet()
        {
            var sm = new StateMachine();
            Assert.Throws<ArgumentNullException>(() => sm.Serializer = null);
        }
    }
}
