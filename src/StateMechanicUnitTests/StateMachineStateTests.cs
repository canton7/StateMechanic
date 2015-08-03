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
    public class StateMachineStateTests
    {
        [Test]
        public void StateMachineStartsInInitialState()
        {
            var sm = new StateMachine("state machine");
            var initialState = sm.CreateInitialState("initial state");
            var state1 = sm.CreateState("state1");

            Assert.AreEqual(initialState, sm.CurrentState);
            Assert.AreEqual(initialState, sm.InitialState);
        }

        [Test]
        public void ChildStateMachineStartsInInitialStateIfParentStateIsInitialState()
        {
            var parent = new StateMachine("parent");
            var initial = parent.CreateInitialState("initial");
            var child = initial.CreateChildStateMachine("child");
            var childInitial = child.CreateInitialState("childInitial");

            Assert.AreEqual(childInitial, child.CurrentState);
            Assert.AreEqual(childInitial, child.InitialState);
        }

        [Test]
        public void ChildStateMachineStartsInNoStateIfParentStateIsNotInitialState()
        {
            var parent = new StateMachine("parent");
            var initial = parent.CreateInitialState("initial");
            var state1 = parent.CreateState("state1");
            var child = state1.CreateChildStateMachine("child");
            var childInitial = child.CreateInitialState("childInitial");

            Assert.AreEqual(null, child.CurrentState);
            Assert.AreEqual(childInitial, child.InitialState);
        }

        [Test]
        public void ResetResetsStateOfChildStateMachines()
        {
            var parent = new StateMachine("parent");
            var initialState = parent.CreateInitialState("initialState");
            var state1 = parent.CreateState("state1");
            var child = state1.CreateChildStateMachine("child");
            var substate1 = child.CreateInitialState("substate1");
            var substate2 = child.CreateState("substate2");

            var evt = parent.CreateEvent("evt");
            initialState.TransitionOn(evt).To(state1);
            substate1.TransitionOn(evt).To(substate2);

            evt.Fire();
            evt.Fire();

            parent.Reset();

            Assert.IsNull(child.CurrentState);
        }
    }
}
