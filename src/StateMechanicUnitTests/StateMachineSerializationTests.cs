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
    public class StateMachineSerializationTests
    {
        [Test]
        public void SerializesHierarchicalStateMachine()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state1");

            var childSm = state1.CreateChildStateMachine("childSm");
            var childInitial = childSm.CreateInitialState("childInitial");
            var childState1 = childSm.CreateState("childState1");

            var evt = new Event("evt");

            sm.ForceTransition(state1, evt);
            childSm.ForceTransition(childState1, evt);

            var serialized = sm.Serialize();
            Assert.AreEqual("1:state1/childState1", serialized);
        }
    }
}
