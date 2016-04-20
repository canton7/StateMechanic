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
        public void InitialStateAddedToParentStateMachine()
        {
            var stateMachine = new StateMachine("State Machine");
            var state = stateMachine.CreateInitialState("State");
            Assert.AreEqual(state, stateMachine.InitialState);
            Assert.That(stateMachine.States, Is.EquivalentTo(new[] { state }));
        }

        [Test]
        public void StateReferencesParentStateMachine()
        {
            var stateMachine = new StateMachine("State Machine");
            var state = stateMachine.CreateState("State");
            Assert.AreEqual(stateMachine, state.ParentStateMachine);
            Assert.AreEqual(stateMachine, ((IState)state).ParentStateMachine);
        }

        [Test]
        public void TransitionAddedToState()
        {
            var stateMachine = new StateMachine("State Machine");
            var state1 = stateMachine.CreateState("State 1");
            var state2 = stateMachine.CreateState("State 2");
            var evt = new Event("Event");
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
            var evt = new Event<EventData>("Event");
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
            var evt = new Event("Event");
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
            var evt = new Event("Event");
            var subSm = state2.CreateChildStateMachine();
            var state21 = subSm.CreateInitialState("State 2.1");

            state1.TransitionOn(evt).To(state2);
            evt.Fire();

            Assert.AreEqual(state2, stateMachine.CurrentState);
            Assert.AreEqual(state2, ((IStateMachine)stateMachine).CurrentState);

            Assert.AreEqual(state21, subSm.CurrentState);
            Assert.AreEqual(state21, ((IStateMachine)subSm).CurrentState);
        }

        [Test]
        public void StateMachineReportsRecursiveCurrentStateCorrectly()
        {
            var stateMachine = new StateMachine("State Machine");
            var state1 = stateMachine.CreateInitialState("State 1");
            var state2 = stateMachine.CreateState("State 2");
            var evt = new Event("Event");
            var subSm = state2.CreateChildStateMachine();
            var state21 = subSm.CreateInitialState("State 2.1");
            var state22 = subSm.CreateState("State 2.2");

            state1.TransitionOn(evt).To(state2);
            state21.TransitionOn(evt).To(state22);

            Assert.AreEqual(state1, stateMachine.CurrentChildState);
            Assert.AreEqual(state1, ((IStateMachine)stateMachine).CurrentChildState);

            evt.Fire();

            Assert.AreEqual(state21, stateMachine.CurrentChildState);
            Assert.AreEqual(state21, ((IStateMachine)stateMachine).CurrentChildState);

            evt.Fire();

            Assert.AreEqual(state22, stateMachine.CurrentChildState);
            Assert.AreEqual(state22, ((IStateMachine)stateMachine).CurrentChildState);
        }

        [Test]
        public void StateReportsIsCurrentCorrectly()
        {
            var stateMachine = new StateMachine("State Machine");
            var state1 = stateMachine.CreateInitialState("State 1");
            var state2 = stateMachine.CreateState("State ");
            var evt = new Event("Event");
            var subSm = state2.CreateChildStateMachine();
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

        [Test]
        public void StateMachineReportsIsActiveCorrectly()
        {
            var parent = new StateMachine();
            var state1 = parent.CreateInitialState("state1");
            var state2 = parent.CreateState("state2");
            var state2Child = state2.CreateChildStateMachine("childSm");
            var state21 = state2Child.CreateInitialState("state21");

            var evt = new Event("evt");

            state1.TransitionOn(evt).To(state2);

            Assert.True(parent.IsActive);
            Assert.False(state2Child.IsActive);

            evt.Fire();

            Assert.True(parent.IsActive);
            Assert.True(state2Child.IsActive);
        }

        [Test]
        public void StateMachineReportsIsChildOfCorrectly()
        {
            var sm1 = new StateMachine("sm1");
            var state11 = sm1.CreateInitialState("state11");

            var sm2 = new StateMachine("sm2");
            var state21 = sm2.CreateInitialState("state21");
            var state22 = sm2.CreateState("state22");
            var sm22 = state22.CreateChildStateMachine("sm22");
            var state221 = sm22.CreateInitialState("state221");

            Assert.False(sm1.IsChildOf(sm2));
            Assert.False(sm2.IsChildOf(sm1));

            Assert.False(sm2.IsChildOf(sm22));
            Assert.True(sm22.IsChildOf(sm2));
        }

        [Test]
        public void StateMachineReportsParentStateCorrectly()
        {
            var sm = new StateMachine();
            var state1 = sm.CreateInitialState("state1");
            var subSm = state1.CreateChildStateMachine("childSm");
            var state11 = subSm.CreateInitialState("state11");

            Assert.Null(sm.ParentState);
            Assert.Null(((IStateMachine)sm).ParentState);

            Assert.AreEqual(state1, subSm.ParentState);
            Assert.AreEqual(state1, ((IStateMachine)subSm).ParentState);
        }

        [Test]
        public void StateMachineReportsParentStateMachineCorrectly()
        {
            var sm = new StateMachine();
            var state1 = sm.CreateInitialState("state1");
            var subSm = state1.CreateChildStateMachine("childSm");
            var state11 = subSm.CreateInitialState("state11");

            Assert.Null(sm.ParentStateMachine);
            Assert.Null(((IStateMachine)sm).ParentStateMachine);

            Assert.AreEqual(sm, subSm.ParentStateMachine);
            Assert.AreEqual(sm, ((IStateMachine)subSm).ParentStateMachine);
        }

        [Test]
        public void StateMachineReportsTopmostStateMachineCorrectly()
        {
            var sm = new StateMachine();
            var state1 = sm.CreateInitialState("state1");
            var subSm = state1.CreateChildStateMachine("childSm");
            var state11 = subSm.CreateInitialState("state11");
            var subSubSm = state11.CreateChildStateMachine("subSubSm");
            var state111 = subSubSm.CreateInitialState("state111");

            Assert.AreEqual(sm, sm.TopmostStateMachine);
            Assert.AreEqual(sm, ((IStateMachine)sm).TopmostStateMachine);

            Assert.AreEqual(sm, subSm.TopmostStateMachine);
            Assert.AreEqual(sm, ((IStateMachine)subSm).TopmostStateMachine);

            Assert.AreEqual(sm, subSubSm.TopmostStateMachine);
            Assert.AreEqual(sm, ((IStateMachine)subSubSm).TopmostStateMachine);
        }

        [Test]
        public void TransitionReportsCorrectInfo()
        {
            Action<TransitionInfo<State>> handler = i => { };
            Func<TransitionInfo<State>, bool> guard = i => true;
            var sm = new StateMachine();
            var state1 = sm.CreateInitialState("state1");
            var state2 = sm.CreateState("state2");
            var evt = new Event();
            var transition = state1.TransitionOn(evt).To(state2).WithHandler(handler).WithGuard(guard);

            Assert.AreEqual(state1, transition.From);
            Assert.AreEqual(state2, transition.To);
            Assert.AreEqual(evt, transition.Event);
            Assert.AreEqual(evt, ((ITransition<State>)transition).Event);
            Assert.False(transition.IsInnerTransition);
            Assert.False(((ITransition<State>)transition).IsDynamicTransition);
            Assert.AreEqual(handler, transition.Handler);
            Assert.True(transition.HasGuard);
            Assert.AreEqual(guard, transition.Guard);
        }

        [Test]
        public void TransitionWithEventDataReportsCorrectInfo()
        {
            Action<TransitionInfo<State, string>> handler = i => { };
            Func<TransitionInfo<State, string>, bool> guard = i => true;
            var sm = new StateMachine();
            var state1 = sm.CreateInitialState("state1");
            var state2 = sm.CreateState("state2");
            var evt = new Event<string>();
            var transition = state1.TransitionOn(evt).To(state2).WithHandler(handler).WithGuard(guard);

            Assert.AreEqual(state1, transition.From);
            Assert.AreEqual(state2, transition.To);
            Assert.AreEqual(evt, transition.Event);
            Assert.AreEqual(evt, ((ITransition<State>)transition).Event);
            Assert.False(transition.IsInnerTransition);
            Assert.False(((ITransition<State>)transition).IsDynamicTransition);
            Assert.AreEqual(handler, transition.Handler);
            Assert.True(transition.HasGuard);
            Assert.AreEqual(guard, transition.Guard);
        }
    }
}
