using Moq;
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
    public class StateMachineSynchronizerTests
    {
        [Test]
        public void EventFireCallsSynchronizerEventFire()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var evt = sm.CreateEvent("evt");
            initial.TransitionOn(evt).To(initial);

            var synchronizer = new Mock<IStateMachineSynchronizer>();
            sm.Synchronizer = synchronizer.Object;

            evt.Fire();

            synchronizer.Verify(x => x.FireEvent(It.IsAny<Func<bool>>(), EventFireMethod.Fire));
        }

        [Test]
        public void EventTryFireCallsSynchronizerEventFire()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var evt = sm.CreateEvent("evt");
            initial.TransitionOn(evt).To(initial);

            var synchronizer = new Mock<IStateMachineSynchronizer>();
            sm.Synchronizer = synchronizer.Object;

            evt.TryFire();

            synchronizer.Verify(x => x.FireEvent(It.IsAny<Func<bool>>(), EventFireMethod.TryFire));
        }

        [Test]
        public void StateMachineDoesNotFireEventUntilFuncIsInvoked()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state1");
            var evt = sm.CreateEvent("evt");
            initial.TransitionOn(evt).To(state1);

            var synchronizer = new Mock<IStateMachineSynchronizer>();
            Func<bool> fireFunc = null;
            synchronizer.Setup(x => x.FireEvent(It.IsAny<Func<bool>>(), EventFireMethod.Fire))
                .Callback<Func<bool>, EventFireMethod>((func, _) => fireFunc = func);
            sm.Synchronizer = synchronizer.Object;

            evt.Fire();

            Assert.AreEqual(initial, sm.CurrentState);
            Assert.NotNull(fireFunc);

            Assert.True(fireFunc());

            Assert.AreEqual(state1, sm.CurrentState);
        }

        [Test]
        public void FuncThrowsExceptionIfEventFireFailed()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var evt = sm.CreateEvent("evt");

            var synchronizer = new Mock<IStateMachineSynchronizer>();
            Func<bool> fireFunc = null;
            synchronizer.Setup(x => x.FireEvent(It.IsAny<Func<bool>>(), EventFireMethod.Fire))
                .Callback<Func<bool>, EventFireMethod>((func, _) => fireFunc = func);
            sm.Synchronizer = synchronizer.Object;

            evt.Fire();

            Assert.Throws<TransitionNotFoundException>(() => fireFunc());
        }

        [Test]
        public void ExceptionFromFireEventIsPropagatedBackToEventFire()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var evt = sm.CreateEvent("evt");

            var exception = new Exception("foo");

            var synchronizer = new Mock<IStateMachineSynchronizer>();
            synchronizer.Setup(x => x.FireEvent(It.IsAny<Func<bool>>(), EventFireMethod.Fire))
                .Callback(() => { throw exception; });
            sm.Synchronizer = synchronizer.Object;

            var e = Assert.Throws<Exception>(() => evt.Fire());
            Assert.AreEqual(exception, e);
        }

        [Test]
        public void FuncReturnsTrueIfEventTryFireSucceeded()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state1");
            var evt = sm.CreateEvent("evt");
            initial.TransitionOn(evt).To(state1);

            var synchronizer = new Mock<IStateMachineSynchronizer>();
            Func<bool> fireFunc = null;
            synchronizer.Setup(x => x.FireEvent(It.IsAny<Func<bool>>(), EventFireMethod.TryFire))
                .Callback<Func<bool>, EventFireMethod>((func, _) => fireFunc = func);
            sm.Synchronizer = synchronizer.Object;

            evt.TryFire();

            Assert.True(fireFunc());
        }

        [Test]
        public void FuncReturnsTrueIfEventTryFireFailed()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var evt = sm.CreateEvent("evt");

            var synchronizer = new Mock<IStateMachineSynchronizer>();
            Func<bool> fireFunc = null;
            synchronizer.Setup(x => x.FireEvent(It.IsAny<Func<bool>>(), EventFireMethod.TryFire))
                .Callback<Func<bool>, EventFireMethod>((func, _) => fireFunc = func);
            sm.Synchronizer = synchronizer.Object;

            evt.TryFire();

            Assert.False(fireFunc());
        }

        [Test]
        public void TrueReturnValueFromFireEventPropagatedToTryFire()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var evt = sm.CreateEvent("evt");

            var synchronizer = new Mock<IStateMachineSynchronizer>();
            synchronizer.Setup(x => x.FireEvent(It.IsAny<Func<bool>>(), EventFireMethod.TryFire))
                .Returns(true);
            sm.Synchronizer = synchronizer.Object;

            Assert.True(evt.TryFire());
        }

        [Test]
        public void FalseReturnValueFromFireEventPropagatedToTryFire()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var evt = sm.CreateEvent("evt");

            var synchronizer = new Mock<IStateMachineSynchronizer>();
            synchronizer.Setup(x => x.FireEvent(It.IsAny<Func<bool>>(), EventFireMethod.TryFire))
                .Returns(false);
            sm.Synchronizer = synchronizer.Object;

            Assert.False(evt.TryFire());
        }

        [Test]
        public void FalseReturnValueDoesNotCauseExceptionToBeThrownByFire()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var evt = sm.CreateEvent("evt");

            var synchronizer = new Mock<IStateMachineSynchronizer>();
            synchronizer.Setup(x => x.FireEvent(It.IsAny<Func<bool>>(), EventFireMethod.TryFire))
                .Returns(false);
            sm.Synchronizer = synchronizer.Object;

            Assert.DoesNotThrow(() => evt.Fire());
        }

        [Test]
        public void ForceTransitionCallsSynchronizerForceTransition()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var evt = sm.CreateEvent("evt");

            var synchronizer = new Mock<IStateMachineSynchronizer>();
            sm.Synchronizer = synchronizer.Object;

            sm.ForceTransition(initial, evt);

            synchronizer.Verify(x => x.ForceTransition(It.IsAny<Action>()));
        }

        [Test]
        public void ForcedTransitionDoesNotOccurUntilActionInvoked()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state1");
            var evt = sm.CreateEvent("evt");

            var synchronizer = new Mock<IStateMachineSynchronizer>();
            sm.Synchronizer = synchronizer.Object;

            Action forceAction = null;
            synchronizer.Setup(x => x.ForceTransition(It.IsAny<Action>()))
                .Callback<Action>(a => forceAction = a);

            sm.ForceTransition(state1, evt);

            Assert.AreEqual(initial, sm.CurrentState);
            Assert.NotNull(forceAction);

            forceAction();

            Assert.AreEqual(state1, sm.CurrentState);
        }

        [Test]
        public void ResetCallsSynchronizerReset()
        {
            var synchronizer = new Mock<IStateMachineSynchronizer>();
            var sm = new StateMachine("sm");
            sm.Synchronizer = synchronizer.Object;

            sm.Reset();

            synchronizer.Verify(x => x.Reset(It.IsAny<Action>()));
        }

        [Test]
        public void StateMachineDoesNotResetUntilActionIsInvoked()
        {
            // Need to transition, so we can tell when it's reset
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state1");
            var evt = sm.CreateEvent("evt");
            initial.TransitionOn(evt).To(state1);
            evt.Fire();

            var synchronizer = new Mock<IStateMachineSynchronizer>();
            sm.Synchronizer = synchronizer.Object;

            Action resetAction = null;
            synchronizer.Setup(x => x.Reset(It.IsAny<Action>())).Callback((Action action) => resetAction = action);

            sm.Reset();

            Assert.AreEqual(state1, sm.CurrentState);
            Assert.NotNull(resetAction);

            resetAction();

            Assert.AreEqual(initial, sm.CurrentState);
        }
    }
}
