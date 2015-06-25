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
    public class CreateStateTests
    {
        private StateMachine stateMachine;

        [SetUp]
        public void SetUp()
        {
            this.stateMachine = new StateMachine("State Machine");
        }

        [Test]
        public void StateCreatedWithCorrectName()
        {
            var state = this.stateMachine.CreateState("State");
            Assert.AreEqual("State", state.Name);
        }

        [Test]
        public void StateAddedToParentStateMachine()
        {
            var state = this.stateMachine.CreateState("State");
            Assert.That(this.stateMachine.States, Is.EquivalentTo(new[] { state }));
        }

        [Test]
        public void StateReferencesParentStateMachine()
        {
            var state = this.stateMachine.CreateState("State");
            Assert.AreEqual(this.stateMachine, state.ParentStateMachine);
        }
    }
}
