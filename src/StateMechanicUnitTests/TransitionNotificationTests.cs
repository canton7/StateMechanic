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
    public class TransitionNotificationTests
    {
        [Test]
        public void TransitionRaisedWhenTransitionOnParent()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state1");
            var evt = new Event("evt");
            initial.TransitionOn(evt).To(state1);

            TransitionEventArgs<State> ea = null;

            sm.Transition += (o, e) =>
            {
                ea = e;
            };

            evt.Fire();

            Assert.NotNull(ea);
            Assert.AreEqual(initial, ea.From);
            Assert.AreEqual(state1, ea.To);
            Assert.AreEqual(evt, ea.Event);
            Assert.AreEqual(sm, ea.StateMachine);
            Assert.False(ea.IsInnerTransition);
            Assert.AreEqual(EventFireMethod.Fire, ea.EventFireMethod);
        }

        [Test]
        public void TransitionRaisedWhenInnerSelfTransitionOnParent()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var evt = new Event("evt");
            initial.InnerSelfTransitionOn(evt);

            TransitionEventArgs<State> ea = null;
            sm.Transition += (o, e) =>
            {
                ea = e;
            };

            evt.TryFire();

            Assert.NotNull(ea);
            Assert.AreEqual(initial, ea.From);
            Assert.AreEqual(initial, ea.To);
            Assert.AreEqual(evt, ea.Event);
            Assert.True(ea.IsInnerTransition);
            Assert.AreEqual(EventFireMethod.TryFire, ea.EventFireMethod);
        }

        [Test]
        public void TransitionRaisedWhenTransitionOnChild()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var child = initial.CreateChildStateMachine();
            var childInitial = child.CreateInitialState("childInitial");
            var childState1 = child.CreateState("childState1");
            var evt = new Event("evt");
            childInitial.TransitionOn(evt).To(childState1);

            TransitionEventArgs<State> ea = null;

            sm.Transition += (o, e) =>
            {
                ea = e;
            };

            evt.Fire();

            Assert.NotNull(ea);
            Assert.AreEqual(childInitial, ea.From);
            Assert.AreEqual(childState1, ea.To);
            Assert.AreEqual(evt, ea.Event);
            Assert.AreEqual(child, ea.StateMachine);
            Assert.False(ea.IsInnerTransition);
            Assert.AreEqual(EventFireMethod.Fire, ea.EventFireMethod);
        }

        [Test]
        public void TransitionRaisedWhenInnerSelfTransitionOnChild()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var child = initial.CreateChildStateMachine();
            var childInitial = child.CreateInitialState("childInitial");
            var evt = new Event("evt");
            childInitial.InnerSelfTransitionOn(evt);

            TransitionEventArgs<State> ea = null;

            sm.Transition += (o, e) =>
            {
                ea = e;
            };

            evt.Fire();

            Assert.NotNull(ea);
            Assert.AreEqual(childInitial, ea.From);
            Assert.AreEqual(childInitial, ea.To);
            Assert.AreEqual(evt, ea.Event);
            Assert.True(ea.IsInnerTransition);
            Assert.AreEqual(EventFireMethod.Fire, ea.EventFireMethod);
        }

        [Test]
        public void TransitionNotFoundRaisedWhenTransitionNotFoundOnParent()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state 1");
            var evt = new Event("evt");

            state1.InnerSelfTransitionOn(evt);

            TransitionNotFoundEventArgs<State> ea = null;
            sm.TransitionNotFound += (o, e) =>
            {
                ea = e;
            };

            evt.TryFire();

            Assert.NotNull(ea);
            Assert.AreEqual(evt, ea.Event);
            Assert.AreEqual(initial, ea.From);
            Assert.AreEqual(sm, ea.StateMachine);
            Assert.AreEqual(EventFireMethod.TryFire, ea.EventFireMethod);
        }

        [Test]
        public void TransitionNotFoundRaisedWhenTransitionNotFoundOnChild()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state 1");
            var child = initial.CreateChildStateMachine();
            var childInitial = child.CreateInitialState("childInitial");
            var evt = new Event("evt");

            state1.InnerSelfTransitionOn(evt);

            TransitionNotFoundEventArgs<State> ea = null;
            sm.TransitionNotFound += (o, e) =>
            {
                ea = e;
            };

            evt.TryFire();

            Assert.NotNull(ea);
            Assert.AreEqual(evt, ea.Event);
            Assert.AreEqual(initial, ea.From);
            Assert.AreEqual(sm, ea.StateMachine);
            Assert.AreEqual(EventFireMethod.TryFire, ea.EventFireMethod);
        }

        [Test]
        public void TransitionRaisedWhenForcedTransition()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state1");
            var evt = new Event("evt");

            TransitionEventArgs<State> ea = null;
            sm.Transition += (o, e) => ea = e;

            sm.ForceTransition(state1, evt);

            Assert.NotNull(ea);
            Assert.AreEqual(initial, ea.From);
            Assert.AreEqual(state1, ea.To);
            Assert.AreEqual(evt, ea.Event);
            Assert.False(ea.IsInnerTransition);
            Assert.AreEqual(EventFireMethod.Fire, ea.EventFireMethod);
        }
    }
}
