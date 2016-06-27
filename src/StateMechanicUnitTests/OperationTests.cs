using NUnit.Framework;
using StateMechanic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StateMechanicUnitTests
{
    [TestFixture]
    public class OperationTests
    {
        [Test]
        public void OperationCompletesWhenASuccessStateIsReached()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var doingOperation = sm.CreateState("doingOperation");
            var completedOperation = sm.CreateState("completedOperation");
            var evt = new Event("evt");

            initial.TransitionOn(evt).To(doingOperation);
            doingOperation.TransitionOn(evt).To(completedOperation);

            var op = new Operation<State>(evt, doingOperation, completedOperation);

            var task = op.TryFireAsync();

            Assert.False(task.IsCompleted);
            Assert.AreEqual(doingOperation, sm.CurrentState);

            evt.TryFire();

            Assert.True(task.IsCompleted);
            Assert.True(task.Result);
        }

        [Test]
        public void OperationFailsIfANonSuccessStateIsReached()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var doingOperation = sm.CreateState("doingOperation");
            var completedOperation = sm.CreateState("completedOperation");
            var operationFailed = sm.CreateState("operationFailed");
            var evt = new Event("evt");

            initial.TransitionOn(evt).To(doingOperation);
            doingOperation.TransitionOn(evt).To(operationFailed);

            var op = new Operation<State>(evt, doingOperation, completedOperation);

            var task = op.TryFireAsync();

            Assert.False(task.IsCompleted);
            Assert.AreEqual(doingOperation, sm.CurrentState);

            evt.TryFire();

            Assert.True(task.IsFaulted);
            var e = Assert.Throws<OperationFailedException>(() => task.GetAwaiter().GetResult());
            Assert.AreEqual(doingOperation, e.OperationState);
            Assert.AreEqual(operationFailed, e.ActualState);
            Assert.That(e.SuccessStates, Is.EquivalentTo(new[] { completedOperation }));
        }

        [Test]
        public void TryFireReturnsFalseIfEventTryFireReturnedFalse()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var state2 = sm.CreateState("state2");
            var evt = new Event("evt");

            state2.TransitionOn(evt).To(state2);

            var op = new Operation<State>(evt, state2);

            var task = op.TryFireAsync();

            Assert.True(task.IsCompleted);
            Assert.False(task.Result);
        }

        [Test]
        public void FireThrowsIfEventFireThrows()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var state2 = sm.CreateState("state2");
            var evt = new Event("evt");

            state2.TransitionOn(evt).To(state2);

            var op = new Operation<State>(evt, state2);

            var task = op.FireAsync();

            Assert.True(task.IsFaulted);
            Assert.Throws<TransitionNotFoundException>(() => task.GetAwaiter().GetResult());
        }

        [Test]
        public void CancellationTokenCancelsOperation()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var doingOperation = sm.CreateState("doingOperation");
            var completedOperation = sm.CreateState("completedOperation");
            var evt = new Event("evt");

            initial.TransitionOn(evt).To(doingOperation);
            doingOperation.TransitionOn(evt).To(completedOperation);

            var cts = new CancellationTokenSource();

            var op = new Operation<State>(evt, doingOperation, completedOperation);

            var task = op.TryFireAsync(cts.Token);

            Assert.False(task.IsCompleted);
            Assert.False(task.IsCanceled);

            cts.Cancel();

            Assert.True(task.IsCanceled);

            // Check nothing explodes
            evt.Fire();
        }
    }
}
