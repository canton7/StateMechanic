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
    public class ResetTests
    {
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
