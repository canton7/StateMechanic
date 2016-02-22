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
            var sm = new StateMachine<State>("State Machine");
            var state = sm.CreateInitialState("Initial State");
            var subSm = state.CreateChildStateMachine();
            var subState = subSm.CreateInitialState("Child Initial State");
            var evt = sm.CreateEvent("Event");
            
            var e = Assert.Throws<InvalidStateTransitionException>(() => subState.TransitionOn(evt).To(state));
            Assert.AreEqual(subState, e.From);
            Assert.AreEqual(state, e.To);
        }

        [Test]
        public void ThrowsIfChildStateUsedInParentStateMachine()
        {
            var sm = new StateMachine<State>("State Machine");
            var state = sm.CreateInitialState("Initial State");
            var subSm = state.CreateChildStateMachine();
            var subState = subSm.CreateInitialState("Child Initial State");
            var evt = sm.CreateEvent("Event");

            var e = Assert.Throws<InvalidStateTransitionException>(() => state.TransitionOn(evt).To(subState));
            Assert.AreEqual(state, e.From);
            Assert.AreEqual(subState, e.To);
        }

        [Test]
        public void ThrowsIfChildEventUsedInParentStateMachine()
        {
            var sm = new StateMachine<State>("State Machine");
            var state = sm.CreateInitialState("Initial State");
            var subSm = state.CreateChildStateMachine();
            var evt = subSm.CreateEvent("Event");

            var e = Assert.Throws<InvalidEventTransitionException>(() => state.TransitionOn(evt).To(state));
            Assert.AreEqual(state, e.From);
            Assert.AreEqual(evt, e.Event);
        }

        [Test]
        public void DoesNotThrowIfParentEventUsedInChildStateMachine()
        {
            var sm = new StateMachine<State>("State Machine");
            var state = sm.CreateInitialState("Initial State");
            var subSm = state.CreateChildStateMachine();
            var subState = subSm.CreateInitialState("Child Initial State");
            var evt = sm.CreateEvent("Event");

            Assert.DoesNotThrow(() => subState.TransitionOn(evt).To(subState));
        }

        [Test]
        public void ThrowsIfForcedTransitionToAStateBelongingToAChildStateMachine()
        {
            var sm = new StateMachine<State>("State Machine");
            var evt = sm.CreateEvent("Event");
            var state = sm.CreateInitialState("Initial State");
            var subSm = state.CreateChildStateMachine();
            var subState = subSm.CreateInitialState("Child Initial State");
            
            Assert.Throws<InvalidStateTransitionException>(() => sm.ForceTransition(subState, evt));
        }

        [Test]
        public void ThrowsIfForcedTransitionOnAChildEvent()
        {
            var sm = new StateMachine<State>("State Machine");
            var state1 = sm.CreateInitialState("Initial State");
            var state2 = sm.CreateState("State 2");
            var subSm = state1.CreateChildStateMachine();
            var evt = subSm.CreateEvent("Event");

            Assert.Throws<InvalidEventTransitionException>(() => sm.ForceTransition(state2, evt));
        }

        [Test]
        public void DoesNotThrowIfForcedTransitionOnAParentEvent()
        {
            var sm = new StateMachine<State>("State Machine");
            var evt = sm.CreateEvent("Event");
            var state = sm.CreateInitialState("Initial State");
            var subSm = state.CreateChildStateMachine();
            var subState1 = subSm.CreateInitialState("Child Initial State");
            var subState2 = subSm.CreateState("Sub State 2");

            subSm.ForceTransition(subState2, evt);
        }

        [Test]
        public void ThrowsIfEventFiredOnStateMachineThatIsNotActive()
        {
            var sm = new StateMachine<State>("State Machine");
            var state1 = sm.CreateInitialState("Initial State");
            var state2 = sm.CreateState("State 2");
            var subSm = state2.CreateChildStateMachine();
            var evt = subSm.CreateEvent("Event");
            var subState1 = subSm.CreateInitialState("Child Initial State");
            subState1.InnerSelfTransitionOn(evt);

            Assert.Throws<InvalidOperationException>(() => evt.Fire());
        }

        [Test]
        public void ThrowsIfEventFiredAndInitialStateNotSet()
        {
            var sm = new StateMachine<State>("State Machine");
            var state1 = sm.CreateState("Initial State");
            var evt = sm.CreateEvent("Event");

            Assert.Throws<InvalidOperationException>(() => evt.Fire());
        }

        [Test]
        public void ThrowsIfInitialStateSetTwice()
        {
            var sm = new StateMachine<State>("State machine");
            var state1 = sm.CreateInitialState("State 1");
            Assert.Throws<InvalidOperationException>(() => sm.CreateInitialState("State 2"));
        }
    }
}
