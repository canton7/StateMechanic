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
        private class StateData
        {
            public int Foo { get; set; }
        }

        [Test]
        public void StateCreatedWithCorrectName()
        {
            var stateMachine = new StateMachine("State Machine");
            var state = stateMachine.CreateState("State");
            Assert.AreEqual("State", state.Name);
        }

        [Test]
        public void StateTCreatedWithCorrectName()
        {
            var stateMachine = new StateMachine<StateData>("State Machine");
            var state = stateMachine.CreateState("StateT", new StateData());
            Assert.AreEqual("StateT", state.Name);
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
    }
}
