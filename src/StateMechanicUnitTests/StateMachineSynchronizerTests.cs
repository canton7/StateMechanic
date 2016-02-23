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
            var sm = new StateMachine<State>("sm");
            var initial = sm.CreateInitialState("initial");
            var evt = new Event("evt");
            initial.TransitionOn(evt).To(initial);

            var synchronizer = new Mock<IStateMachineSynchronizer>();
            sm.Synchronizer = synchronizer.Object;

            evt.Fire();

            synchronizer.Verify(x => x.FireEvent(It.IsAny<Func<bool>>(), EventFireMethod.Fire));
        }

        [Test]
        public void EventTryFireCallsSynchronizerEventFire()
        {
            var sm = new StateMachine<State>("sm");
            var initial = sm.CreateInitialState("initial");
            var evt = new Event("evt");
            initial.TransitionOn(evt).To(initial);

            var synchronizer = new Mock<IStateMachineSynchronizer>();
            sm.Synchronizer = synchronizer.Object;

            evt.TryFire();

            synchronizer.Verify(x => x.FireEvent(It.IsAny<Func<bool>>(), EventFireMethod.TryFire));
        }

        [Test]
        public void StateMachineDoesNotFireEventUntilFuncIsInvoked()
        {
            var sm = new StateMachine<State>("sm");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state1");
            var evt = new Event("evt");
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
            var sm = new StateMachine<State>("sm");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state 1");
            var evt = new Event("evt");

            state1.InnerSelfTransitionOn(evt);

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
            var sm = new StateMachine<State>("sm");
            var initial = sm.CreateInitialState("initial");
            var state2 = sm.CreateState("State 2");
            var evt = new Event("evt");

            state2.TransitionOn(evt).To(state2);

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
            var sm = new StateMachine<State>("sm");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state1");
            var evt = new Event("evt");
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
            var sm = new StateMachine<State>("sm");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state 1");
            var evt = new Event("evt");

            state1.InnerSelfTransitionOn(evt);

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
            var sm = new StateMachine<State>("sm");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state 1");
            var evt = new Event("evt");

            state1.InnerSelfTransitionOn(evt);

            var synchronizer = new Mock<IStateMachineSynchronizer>();
            synchronizer.Setup(x => x.FireEvent(It.IsAny<Func<bool>>(), EventFireMethod.TryFire))
                .Returns(true);
            sm.Synchronizer = synchronizer.Object;

            Assert.True(evt.TryFire());
        }

        [Test]
        public void FalseReturnValueFromFireEventPropagatedToTryFire()
        {
            var sm = new StateMachine<State>("sm");
            var initial = sm.CreateInitialState("initial");
            var evt = new Event("evt");

            var synchronizer = new Mock<IStateMachineSynchronizer>();
            synchronizer.Setup(x => x.FireEvent(It.IsAny<Func<bool>>(), EventFireMethod.TryFire))
                .Returns(false);
            sm.Synchronizer = synchronizer.Object;

            Assert.False(evt.TryFire());
        }

        [Test]
        public void FalseReturnValueDoesNotCauseExceptionToBeThrownByFire()
        {
            var sm = new StateMachine<State>("sm");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("State 1");
            var evt = new Event("evt");

            state1.InnerSelfTransitionOn(evt);

            var synchronizer = new Mock<IStateMachineSynchronizer>();
            synchronizer.Setup(x => x.FireEvent(It.IsAny<Func<bool>>(), EventFireMethod.TryFire))
                .Returns(false);
            sm.Synchronizer = synchronizer.Object;

            Assert.DoesNotThrow(() => evt.Fire());
        }

        [Test]
        public void EventsFiredFromHandlersCallsSynchronizer()
        {
            var sm = new StateMachine<State>("sm");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state1");
            var state2 = sm.CreateState("state2");
            var evt = new Event("evt");
            initial.TransitionOn(evt).To(state1).WithHandler(i => evt.Fire());
            state1.TransitionOn(evt).To(state2);

            var synchornizer = new Mock<IStateMachineSynchronizer>();
            sm.Synchronizer = synchornizer.Object;

            // Fire the first, capture the second
            bool calledOnce = false;
            Func<bool> fireFunc = null;
            synchornizer.Setup(x => x.FireEvent(It.IsAny<Func<bool>>(), EventFireMethod.Fire))
                .Callback<Func<bool>, EventFireMethod>((func, _) =>
                {
                    if (!calledOnce)
                    {
                        // Need to set this before invoking func(), as we'll recurse
                        calledOnce = true;
                        func();
                    }
                    else
                    {
                        fireFunc = func;
                    }
                });

            evt.Fire();

            Assert.AreEqual(state1, sm.CurrentState);
            Assert.NotNull(fireFunc);

            fireFunc();

            Assert.AreEqual(state2, sm.CurrentState);
        }

        [Test]
        public void ForceTransitionCallsSynchronizerForceTransition()
        {
            var sm = new StateMachine<State>("sm");
            var initial = sm.CreateInitialState("initial");
            var evt = new Event("evt");

            var synchronizer = new Mock<IStateMachineSynchronizer>();
            sm.Synchronizer = synchronizer.Object;

            sm.ForceTransition(initial, evt);

            synchronizer.Verify(x => x.ForceTransition(It.IsAny<Action>()));
        }

        [Test]
        public void ForcedTransitionDoesNotOccurUntilActionInvoked()
        {
            var sm = new StateMachine<State>("sm");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state1");
            var evt = new Event("evt");

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
            var sm = new StateMachine<State>("sm");
            sm.Synchronizer = synchronizer.Object;

            sm.Reset();

            synchronizer.Verify(x => x.Reset(It.IsAny<Action>()));
        }

        [Test]
        public void StateMachineDoesNotResetUntilActionIsInvoked()
        {
            // Need to transition, so we can tell when it's reset
            var sm = new StateMachine<State>("sm");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state1");
            var evt = new Event("evt");
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
