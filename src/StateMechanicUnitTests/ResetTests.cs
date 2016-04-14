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
        public void ResettingStateMachineRemovesFault()
        {
            var exception = new Exception("foo");
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial").WithEntry(i => { throw exception; });
            var evt = new Event("evt");
            initial.TransitionOn(evt).To(initial);

            Assert.Throws<TransitionFailedException>(() => evt.TryFire());

            sm.Reset();
            Assert.False(sm.IsFaulted);
            Assert.Null(sm.Fault);
        }

        [Test]
        public void ResetResetsCurrentStateOfStateMachine()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state1");
            var evt = new Event("evt");
            initial.TransitionOn(evt).To(state1);

            evt.Fire();

            sm.Reset();

            Assert.AreEqual(initial, sm.CurrentState);
        }

        [Test]
        public void ResetResetsStateOfChildStateMachines()
        {
            var parent = new StateMachine("parent");
            var initialState = parent.CreateInitialState("initialState");
            var state1 = parent.CreateState("state1");
            var child = state1.CreateChildStateMachine();
            var substate1 = child.CreateInitialState("substate1");
            var substate2 = child.CreateState("substate2");

            var evt = new Event("evt");
            initialState.TransitionOn(evt).To(state1);
            substate1.TransitionOn(evt).To(substate2);

            evt.Fire();
            evt.Fire();

            parent.Reset();

            Assert.IsNull(child.CurrentState);
        }

        [Test]
        public void ResetEmptiesTransitionQueue()
        {
            var sm = new StateMachine("parent");
            var state1 = sm.CreateInitialState("state1");
            var state2 = sm.CreateState("state2");

            var evt = new Event("evt");

            state1.TransitionOn(evt).To(state2).WithHandler(i =>
            {
                evt.Fire();
                sm.Reset();
            });
            state2.TransitionOn(evt).To(state1);

            evt.Fire();
            
            // Make sure that the queued event didn't get fired
            Assert.AreEqual(state2, sm.CurrentState);

            // Make sure nothing's been queued
            evt.Fire();
            Assert.AreEqual(state1, sm.CurrentState);
        }
    }
}
