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
    public class FaultTests
    {
        private StateMachine<State> stateMachine;

        private State state1;
        private State state2;
        private Event event1;
        private Transition<State> transition12;

        [SetUp]
        public void SetUp()
        {
            this.stateMachine = new StateMachine<State>("State Machine");

            this.state1 = this.stateMachine.CreateInitialState("State 1");
            this.state2 = this.stateMachine.CreateState("State 2");

            this.event1 = this.stateMachine.CreateEvent("Event 1");

            this.transition12 = this.state1.TransitionOn(this.event1).To(this.state2);
        }

        [Test]
        public void ExceptionInExitHandlerPropagatedCorrectly()
        {
            var exception = new Exception("Foo");
            this.state1.ExitHandler = i => { throw exception; };

            var e = Assert.Throws<TransitionFailedException>(() => this.event1.TryFire());
            Assert.AreEqual(this.event1, e.FaultInfo.Event);
            Assert.AreEqual(this.state1, e.FaultInfo.From);
            Assert.AreEqual(this.state2, e.FaultInfo.To);
            Assert.AreEqual(exception, e.FaultInfo.Exception);
            Assert.AreEqual(FaultedComponent.ExitHandler, e.FaultInfo.FaultedComponent);
            Assert.AreEqual(this.stateMachine, e.FaultInfo.StateMachine);
        }

        [Test]
        public void ExceptionInEntryHandlerPropagatedCorrectly()
        {
            var exception = new Exception("Foo");
            this.state2.EntryHandler = i => { throw exception; };

            var e = Assert.Throws<TransitionFailedException>(() => this.event1.TryFire());
            Assert.AreEqual(this.event1, e.FaultInfo.Event);
            Assert.AreEqual(this.state1, e.FaultInfo.From);
            Assert.AreEqual(this.state2, e.FaultInfo.To);
            Assert.AreEqual(exception, e.FaultInfo.Exception);
            Assert.AreEqual(FaultedComponent.EntryHandler, e.FaultInfo.FaultedComponent);
            Assert.AreEqual(this.stateMachine, e.FaultInfo.StateMachine);
        }

        [Test]
        public void ExceptionInGuardDoesNotCauseFault()
        {
            var exception = new Exception("Foo");
            this.transition12.Guard = i => { throw exception; };

            var e = Assert.Throws<Exception>(() => this.event1.TryFire());
            Assert.AreEqual(exception, e);
            Assert.False(this.stateMachine.IsFaulted);
        }

        [Test]
        public void ExceptionInTransitionHandlerPropagatedCorrectly()
        {
            var exception = new Exception("Foo");
            this.transition12.Handler = i => { throw exception; };

            var e = Assert.Throws<TransitionFailedException>(() => this.event1.TryFire());
            Assert.AreEqual(this.event1, e.FaultInfo.Event);
            Assert.AreEqual(this.state1, e.FaultInfo.From);
            Assert.AreEqual(this.state2, e.FaultInfo.To);
            Assert.AreEqual(exception, e.FaultInfo.Exception);
            Assert.AreEqual(FaultedComponent.TransitionHandler, e.FaultInfo.FaultedComponent);
            Assert.AreEqual(this.stateMachine, e.FaultInfo.StateMachine);
        }

        [Test]
        public void StateMachineRaisesEventOnFault()
        {
            StateMachineFaultedEventArgs ea = null;
            this.stateMachine.Faulted += (o, e) => ea = e;

            var exception = new Exception("foo");
            this.state1.ExitHandler = i => { throw exception; };

            Assert.Throws<TransitionFailedException>(() => this.event1.TryFire());

            Assert.NotNull(ea);
            Assert.AreEqual(this.event1, ea.FaultInfo.Event);
            Assert.AreEqual(this.state1, ea.FaultInfo.From);
            Assert.AreEqual(this.state2, ea.FaultInfo.To);
            Assert.AreEqual(exception, ea.FaultInfo.Exception);
            Assert.AreEqual(FaultedComponent.ExitHandler, ea.FaultInfo.FaultedComponent);
            Assert.AreEqual(this.stateMachine, ea.FaultInfo.StateMachine);
        }

        [Test]
        public void FaultedStateMachineReportsCorrectFaultedInfo()
        {
            var exception = new Exception("foo");
            this.state1.ExitHandler = i => { throw exception; };

            Assert.Throws<TransitionFailedException>(() => this.event1.TryFire());

            Assert.True(this.stateMachine.IsFaulted);
            Assert.NotNull(this.stateMachine.Fault);
            Assert.AreEqual(this.event1, this.stateMachine.Fault.Event);
            Assert.AreEqual(this.state1, this.stateMachine.Fault.From);
            Assert.AreEqual(this.state2, this.stateMachine.Fault.To);
            Assert.AreEqual(exception, this.stateMachine.Fault.Exception);
            Assert.AreEqual(FaultedComponent.ExitHandler, this.stateMachine.Fault.FaultedComponent);
            Assert.AreEqual(this.stateMachine, this.stateMachine.Fault.StateMachine);
        }

        [Test]
        public void StateMachineRefusesToFireEventsOnceFaulted()
        {
            var exception = new Exception("foo");
            this.state1.ExitHandler = i => { throw exception; };

            Assert.Throws<TransitionFailedException>(() => this.event1.TryFire());

            var e = Assert.Throws<StateMachineFaultedException>(() => this.event1.TryFire());
            Assert.AreEqual(this.event1, e.FaultInfo.Event);
            Assert.AreEqual(this.state1, e.FaultInfo.From);
            Assert.AreEqual(this.state2, e.FaultInfo.To);
            Assert.AreEqual(exception, e.FaultInfo.Exception);
            Assert.AreEqual(FaultedComponent.ExitHandler, e.FaultInfo.FaultedComponent);
            Assert.AreEqual(this.stateMachine, e.FaultInfo.StateMachine);
        }

        [Test]
        public void StateMachineThrowsFaultExceptionWhenAccessingCurrentStateOnceFaulted()
        {
            var exception = new Exception("foo");
            this.state1.ExitHandler = i => { throw exception; };

            Assert.Throws<TransitionFailedException>(() => this.event1.TryFire());

            var e = Assert.Throws<StateMachineFaultedException>(() => { var x = this.stateMachine.CurrentState; });
            Assert.AreEqual(this.event1, e.FaultInfo.Event);
            Assert.AreEqual(this.state1, e.FaultInfo.From);
            Assert.AreEqual(this.state2, e.FaultInfo.To);
            Assert.AreEqual(exception, e.FaultInfo.Exception);
            Assert.AreEqual(FaultedComponent.ExitHandler, e.FaultInfo.FaultedComponent);
            Assert.AreEqual(this.stateMachine, e.FaultInfo.StateMachine);
        }

        [Test]
        public void FaultingChildStateMachineFaultsParent()
        {
            var childStateMachine = this.state1.CreateChildStateMachine();

            var subState1 = childStateMachine.CreateInitialState("State 1.1");
            var subState2 = childStateMachine.CreateState("State 1.2");
            subState1.TransitionOn(this.event1).To(subState2);

            subState1.ExitHandler = i => { throw new Exception("foo"); };

            Assert.Throws<TransitionFailedException>(() => this.event1.TryFire());
            Assert.True(this.stateMachine.IsFaulted);
        }

        [Test]
        public void ExceptionInExitHandlerFaultsWhenForcedTransition()
        {
            var exception = new Exception("foo");
            this.state1.ExitHandler = i => { throw exception; };

            Assert.Throws<TransitionFailedException>(() => this.stateMachine.ForceTransition(this.state2, this.event1));
            Assert.True(this.stateMachine.IsFaulted);
        }

        [Test]
        public void ExceptionInEntryHandlerFaultsWhenForcedTransition()
        {
            var exception = new Exception("foo");
            this.state2.EntryHandler = i => { throw exception; };

            Assert.Throws<TransitionFailedException>(() => this.stateMachine.ForceTransition(this.state2, this.event1));
            Assert.True(this.stateMachine.IsFaulted);
        }
    }
}
