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
    public class IntrospectionTests
    {
        private class StateData
        {
        }
        private class EventData
        {
        }

        [Test]
        public void StateAddedToParentStateMachine()
        {
            var stateMachine = new StateMachine("State Machine");
            var state = stateMachine.CreateState("State");
            Assert.That(stateMachine.States, Is.EquivalentTo(new[] { state }));
        }

        [Test]
        public void StateTAddedToParentStateMachine()
        {
            var stateMachine = new StateMachine<StateData>("State Machine");
            var state = stateMachine.CreateState("StateT", new StateData());
            Assert.That(stateMachine.States, Is.EquivalentTo(new[] { state }));
        }

        [Test]
        public void InitialStateAddedToParentStateMachine()
        {
            var stateMachine = new StateMachine("State Machine");
            var state = stateMachine.CreateInitialState("State");
            Assert.AreEqual(state, stateMachine.InitialState);
            Assert.That(stateMachine.States, Is.EquivalentTo(new[] { state }));
        }

        [Test]
        public void InitialStateTAddedToParentStateMachine()
        {
            var stateMachine = new StateMachine<StateData>("State Machine");
            var state = stateMachine.CreateInitialState("State", new StateData());
            Assert.AreEqual(state, stateMachine.InitialState);
            Assert.That(stateMachine.States, Is.EquivalentTo(new[] { state }));
        }

        [Test]
        public void StateReferencesParentStateMachine()
        {
            var stateMachine = new StateMachine("State Machine");
            var state = stateMachine.CreateState("State");
            Assert.AreEqual(stateMachine, state.ParentStateMachine);
        }

        [Test]
        public void StateTReferencesParentStateMachine()
        {
            var stateMachine = new StateMachine<StateData>("State Machine");
            var state = stateMachine.CreateState("State", new StateData());
            Assert.AreEqual(stateMachine, state.ParentStateMachine);
        }

        [Test]
        public void TransitionAddedToState()
        {
            var stateMachine = new StateMachine("State Machine");
            var state1 = stateMachine.CreateState("State 1");
            var state2 = stateMachine.CreateState("State 2");
            var evt = stateMachine.CreateEvent("Event");
            state1.TransitionOn(evt).To(state2);

            Assert.AreEqual(1, state1.Transitions.Count);
            Assert.AreEqual(state1, state1.Transitions[0].From);
            Assert.AreEqual(state2, state1.Transitions[0].To);
            Assert.AreEqual(evt, state1.Transitions[0].Event);
            Assert.False(state1.Transitions[0].IsInnerTransition);
        }

        [Test]
        public void TransitionAddedToStateT()
        {
            var stateMachine = new StateMachine<StateData>("State Machine");
            var state1 = stateMachine.CreateState("State 1", new StateData());
            var state2 = stateMachine.CreateState("State 2", new StateData());
            var evt = stateMachine.CreateEvent("Event");
            state1.TransitionOn(evt).To(state2);

            Assert.AreEqual(1, state1.Transitions.Count);
            Assert.AreEqual(state1, state1.Transitions[0].From);
            Assert.AreEqual(state2, state1.Transitions[0].To);
            Assert.AreEqual(evt, state1.Transitions[0].Event);
            Assert.False(state1.Transitions[0].IsInnerTransition);
        }

        [Test]
        public void TransitionTAddedToState()
        {
            var stateMachine = new StateMachine("State Machine");
            var state1 = stateMachine.CreateState("State 1");
            var state2 = stateMachine.CreateState("State 2");
            var evt = stateMachine.CreateEvent<EventData>("Event");
            state1.TransitionOn(evt).To(state2);

            Assert.AreEqual(1, state1.Transitions.Count);
            Assert.AreEqual(state1, state1.Transitions[0].From);
            Assert.AreEqual(state2, state1.Transitions[0].To);
            Assert.AreEqual(evt, state1.Transitions[0].Event);
            Assert.False(state1.Transitions[0].IsInnerTransition);
        }

        [Test]
        public void TransitionTAddedToStateT()
        {
            var stateMachine = new StateMachine<StateData>("State Machine");
            var state1 = stateMachine.CreateState("State 1", new StateData());
            var state2 = stateMachine.CreateState("State 2", new StateData());
            var evt = stateMachine.CreateEvent<EventData>("Event");
            state1.TransitionOn(evt).To(state2);

            Assert.AreEqual(1, state1.Transitions.Count);
            Assert.AreEqual(state1, state1.Transitions[0].From);
            Assert.AreEqual(state2, state1.Transitions[0].To);
            Assert.AreEqual(evt, state1.Transitions[0].Event);
            Assert.False(state1.Transitions[0].IsInnerTransition);
        }

        [Test]
        public void InnerTransitionAddedToState()
        {
            var stateMachine = new StateMachine("State Machine");
            var state1 = stateMachine.CreateState("State 1");
            var evt = stateMachine.CreateEvent("Event");
            state1.InnerSelfTransitionOn(evt);

            Assert.AreEqual(1, state1.Transitions.Count);
            Assert.AreEqual(state1, state1.Transitions[0].From);
            Assert.AreEqual(state1, state1.Transitions[0].To);
            Assert.AreEqual(evt, state1.Transitions[0].Event);
            Assert.True(state1.Transitions[0].IsInnerTransition);
        }

        [Test]
        public void StateMachineReportsCurrentStateCorrectly()
        {
            var stateMachine = new StateMachine("State Machine");
            var state1 = stateMachine.CreateInitialState("State 1");
            var state2 = stateMachine.CreateState("State 2");
            var evt = stateMachine.CreateEvent("Event");
            var subSm = state2.CreateChildStateMachine("Sub State Machine");
            var state21 = subSm.CreateInitialState("State 2.1");

            state1.TransitionOn(evt).To(state2);
            evt.Fire();

            Assert.AreEqual(state2, stateMachine.CurrentState);
            Assert.AreEqual(state21, subSm.CurrentState);
        }

        [Test]
        public void StateMachineReportsRecursiveCurrentStateCorrectly()
        {
            var stateMachine = new StateMachine("State Machine");
            var state1 = stateMachine.CreateInitialState("State 1");
            var state2 = stateMachine.CreateState("State 2");
            var evt = stateMachine.CreateEvent("Event");
            var subSm = state2.CreateChildStateMachine("Sub State Machine");
            var state21 = subSm.CreateInitialState("State 2.1");
            var state22 = subSm.CreateState("State 2.2");

            state1.TransitionOn(evt).To(state2);
            state21.TransitionOn(evt).To(state22);

            Assert.AreEqual(state1, stateMachine.CurrentChildState);

            evt.Fire();

            Assert.AreEqual(state21, stateMachine.CurrentChildState);

            evt.Fire();

            Assert.AreEqual(state22, stateMachine.CurrentChildState);
        }

        [Test]
        public void StateReportsIsCurrentCorrectly()
        {
            var stateMachine = new StateMachine("State Machine");
            var state1 = stateMachine.CreateInitialState("State 1");
            var state2 = stateMachine.CreateState("State ");
            var evt = stateMachine.CreateEvent("Event");
            var subSm = state2.CreateChildStateMachine("Sub State Machine");
            var state21 = subSm.CreateInitialState("State 2.1");
            var state22 = subSm.CreateState("State 2.2");

            state1.TransitionOn(evt).To(state2);
            state21.TransitionOn(evt).To(state22);

            Assert.True(state1.IsCurrent);
            Assert.False(state2.IsCurrent);
            Assert.False(state21.IsCurrent);
            Assert.False(state22.IsCurrent);

            evt.Fire();

            Assert.False(state1.IsCurrent);
            Assert.True(state2.IsCurrent);
            Assert.True(state21.IsCurrent);
            Assert.False(state22.IsCurrent);

            evt.Fire();

            Assert.False(state1.IsCurrent);
            Assert.True(state2.IsCurrent);
            Assert.False(state21.IsCurrent);
            Assert.True(state22.IsCurrent);
        }
    }
}
