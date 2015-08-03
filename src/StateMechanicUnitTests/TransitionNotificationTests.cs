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
        public void NotifiesOfTransition()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state1");
            var evt = sm.CreateEvent("evt");
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
            Assert.False(ea.IsInnerTransition);
        }

        [Test]
        public void NotifiesOfInnerSelfTransition()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var evt = sm.CreateEvent("evt");
            initial.InnerSelfTransitionOn(evt);

            TransitionEventArgs<State> ea = null;
            sm.Transition += (o, e) =>
            {
                ea = e;
            };

            evt.Fire();

            Assert.NotNull(ea);
            Assert.AreEqual(initial, ea.From);
            Assert.AreEqual(initial, ea.To);
            Assert.AreEqual(evt, ea.Event);
            Assert.True(ea.IsInnerTransition);
        }

        [Test]
        public void NotifiesOfTransitionNotFound()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var evt = sm.CreateEvent("evt");

            TransitionNotFoundEventArgs<State> ea = null;
            sm.TransitionNotFound += (o, e) =>
            {
                ea = e;
            };

            evt.TryFire();

            Assert.NotNull(ea);
            Assert.AreEqual(evt, ea.Event);
            Assert.AreEqual(initial, ea.From);
        }

        [Test]
        public void DoesNotNotifyOfTransitionInChildStateMachine()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var child = initial.CreateChildStateMachine("child");
            var childInitial = child.CreateInitialState("childInitial");
            var childState1 = child.CreateState("childState1");
            var evt = child.CreateEvent("evt");
            childInitial.TransitionOn(evt).To(childState1);

            bool fired = false;
            sm.Transition += (o, e) => fired = true;

            evt.Fire();

            Assert.False(fired);
        }

        [Test]
        public void DoesNotNotifyOfTransitionNotFoundInChildStateMachine()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var child = initial.CreateChildStateMachine("child");
            var childInitial = child.CreateInitialState("childInitial");
            var evt = child.CreateEvent("evt");

            bool fired = false;
            sm.TransitionNotFound += (o, e) => fired = true;

            evt.TryFire();

            Assert.False(fired);
        }

        [Test]
        public void GlobalTransitionRaisedWhenTransitionOnParent()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state1");
            var evt = sm.CreateEvent("evt");
            initial.TransitionOn(evt).To(state1);

            TransitionEventArgs<State> ea = null;

            sm.GlobalTransition += (o, e) =>
            {
                ea = e;
            };

            evt.Fire();

            Assert.NotNull(ea);
            Assert.AreEqual(initial, ea.From);
            Assert.AreEqual(state1, ea.To);
            Assert.AreEqual(evt, ea.Event);
            Assert.False(ea.IsInnerTransition);
        }

        [Test]
        public void GlobalTransitionRaisedWhenInnerSelfTransitionOnParent()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var evt = sm.CreateEvent("evt");
            initial.InnerSelfTransitionOn(evt);

            TransitionEventArgs<State> ea = null;
            sm.GlobalTransition += (o, e) =>
            {
                ea = e;
            };

            evt.Fire();

            Assert.NotNull(ea);
            Assert.AreEqual(initial, ea.From);
            Assert.AreEqual(initial, ea.To);
            Assert.AreEqual(evt, ea.Event);
            Assert.True(ea.IsInnerTransition);
        }

        [Test]
        public void GlobalTransitionRaisedWhenTransitionOnChild()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var child = initial.CreateChildStateMachine("child");
            var childInitial = child.CreateInitialState("childInitial");
            var childState1 = child.CreateState("childState1");
            var evt = sm.CreateEvent("evt");
            childInitial.TransitionOn(evt).To(childState1);

            TransitionEventArgs<State> ea = null;

            sm.GlobalTransition += (o, e) =>
            {
                ea = e;
            };

            evt.Fire();

            Assert.NotNull(ea);
            Assert.AreEqual(childInitial, ea.From);
            Assert.AreEqual(childState1, ea.To);
            Assert.AreEqual(evt, ea.Event);
            Assert.False(ea.IsInnerTransition);
        }

        [Test]
        public void GlobalTransitionRaisedWhenInnerSelfTransitionOnChild()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var child = initial.CreateChildStateMachine("child");
            var childInitial = child.CreateInitialState("childInitial");
            var evt = sm.CreateEvent("evt");
            childInitial.InnerSelfTransitionOn(evt);

            TransitionEventArgs<State> ea = null;

            sm.GlobalTransition += (o, e) =>
            {
                ea = e;
            };

            evt.Fire();

            Assert.NotNull(ea);
            Assert.AreEqual(childInitial, ea.From);
            Assert.AreEqual(childInitial, ea.To);
            Assert.AreEqual(evt, ea.Event);
            Assert.True(ea.IsInnerTransition);
        }

        [Test]
        public void GlobalTransitionNotFoundRaisedWhenTransitionNotFoundOnParent()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var evt = sm.CreateEvent("evt");

            TransitionNotFoundEventArgs<State> ea = null;
            sm.GlobalTransitionNotFound += (o, e) =>
            {
                ea = e;
            };

            evt.TryFire();

            Assert.NotNull(ea);
            Assert.AreEqual(evt, ea.Event);
            Assert.AreEqual(initial, ea.From);
        }

        [Test]
        public void GlobalTransitionNotFoundRaisedWhenTransitionNotFoundOnChild()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var child = initial.CreateChildStateMachine("child");
            var childInitial = child.CreateInitialState("childInitial");
            var evt = child.CreateEvent("evt");

            TransitionNotFoundEventArgs<State> ea = null;
            sm.GlobalTransitionNotFound += (o, e) =>
            {
                ea = e;
            };

            evt.TryFire();

            Assert.NotNull(ea);
            Assert.AreEqual(evt, ea.Event);
            Assert.AreEqual(childInitial, ea.From);
        }
    }
}
