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
        public void ResetResetsStateOfChildStateMachines()
        {
            var parent = new StateMachine("parent");
            var state1 = parent.CreateInitialState("state1");
            var child = state1.CreateChildStateMachine("child");
            var substate1 = child.CreateInitialState("substate1");
            var substate2 = child.CreateState("substate2");

            var evt = parent.CreateEvent("evt");
            substate1.TransitionOn(evt).To(substate2);

            evt.Fire();

            parent.Reset();

            Assert.IsNull(child.CurrentState);
        }
    }
}
