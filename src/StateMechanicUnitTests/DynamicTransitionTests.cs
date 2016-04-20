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
    public class DynamicTransitionTests
    {
        [Test]
        public void StateSelectorWhichReturnsNullAbortsTransition()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var evt = new Event("evt");
            initial.TransitionOn(evt).ToDynamic(i => null);
            Assert.False(evt.TryFire());
        }

        [Test]
        public void StateSelectorChoosesNextState()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state1");
            var evt = new Event("evt");
            initial.TransitionOn(evt).ToDynamic(i => state1);

            evt.Fire();
            Assert.AreEqual(state1, sm.CurrentState);
        }

        [Test]
        public void StateSelectorForbidsTransitionToStateFromDifferentStateMachine()
        {
            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var child = initial.CreateChildStateMachine();
            var childInitial = child.CreateInitialState("childInitial");
            var evt = new Event("evt");
            initial.TransitionOn(evt).ToDynamic(i => childInitial);

            var e = Assert.Throws<InvalidStateTransitionException>(() => evt.Fire());
            Assert.AreEqual(initial, e.From);
            Assert.AreEqual(childInitial, e.To);
        }

        [Test]
        public void StateSelectorCausesOuterSelfTransition()
        {
            bool exitCalled = false;
            bool entryCalled = false;

            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial")
                .WithEntry(i => entryCalled = true)
                .WithExit(i => exitCalled = true);
            var evt = new Event("evt");
            initial.TransitionOn(evt).ToDynamic(i => initial);

            evt.Fire();
            Assert.True(entryCalled);
            Assert.True(exitCalled);
        }

        [Test]
        public void DynamicTransitionHasCorrectData()
        {
            var sm = new StateMachine("sm");
            var state1 = sm.CreateInitialState("state1");
            var evt = new Event("evt");

            Func<DynamicSelectorInfo<State>, State> stateSelector = i => null;
            Action<TransitionInfo<State>> handler = i => { };
            var transition = state1.TransitionOn(evt).ToDynamic(stateSelector).WithHandler(handler);
            var iTransition = (ITransition<State>)transition;

            Assert.AreEqual(state1, transition.From);
            Assert.Null(iTransition.To);
            Assert.AreEqual(evt, transition.Event);
            Assert.AreEqual(evt, iTransition.Event);
            Assert.True(iTransition.IsDynamicTransition);
            Assert.False(iTransition.IsInnerTransition);
            Assert.False(iTransition.HasGuard);
            Assert.AreEqual(stateSelector, transition.StateSelector);
            Assert.AreEqual(handler, transition.Handler);
        }

        [Test]
        public void DynamicTransitionStateSelectorIsSettable()
        {
            var sm = new StateMachine("sm");
            var state1 = sm.CreateInitialState("state1");
            var state2 = sm.CreateState("state2");
            var evt = new Event("evt");

            var transition = state1.TransitionOn(evt).ToDynamic(i => state2);
            transition.StateSelector = i => null;

            evt.TryFire();

            Assert.AreEqual(state1, sm.CurrentState);
        }

        [Test]
        public void DynamicTransitionStateSelectorThrowsIfNullIsSet()
        {
            var sm = new StateMachine("sm");
            var state1 = sm.CreateInitialState("state1");
            var evt = new Event<string>("evt");

            Assert.Throws<ArgumentNullException>(() => state1.TransitionOn(evt).To(null));

            var transition = state1.TransitionOn(evt).ToDynamic(i => null);
            Assert.Throws<ArgumentNullException>(() => transition.StateSelector = null);
        }

        [Test]
        public void DynamicTransitionWithEventDataHasCorrectData()
        {
            var sm = new StateMachine("sm");
            var state1 = sm.CreateInitialState("state1");
            var evt = new Event<string>("evt");

            Func<DynamicSelectorInfo<State, string>, State> stateSelector = i => null;
            Action<TransitionInfo<State, string>> handler = i => { };
            var transition = state1.TransitionOn(evt).ToDynamic(stateSelector).WithHandler(handler);
            var iTransition = (ITransition<State>)transition;

            Assert.AreEqual(state1, transition.From);
            Assert.Null(iTransition.To);
            Assert.AreEqual(evt, transition.Event);
            Assert.AreEqual(evt, iTransition.Event);
            Assert.True(iTransition.IsDynamicTransition);
            Assert.False(iTransition.IsInnerTransition);
            Assert.False(iTransition.HasGuard);
            Assert.AreEqual(stateSelector, transition.StateSelector);
            Assert.AreEqual(handler, transition.Handler);
        }

        [Test]
        public void DynamicTransitionWithEventDataStateSelectorIsSettable()
        {
            var sm = new StateMachine("sm");
            var state1 = sm.CreateInitialState("state1");
            var state2 = sm.CreateState("state2");
            var evt = new Event<string>("evt");

            var transition = state1.TransitionOn(evt).ToDynamic(i => state2);
            transition.StateSelector = i => null;

            evt.TryFire("hello");

            Assert.AreEqual(state1, sm.CurrentState);
        }

        [Test]
        public void DynamicTransitionWithEventDataStateSelectorThrowsIfNullIsSet()
        {
            var sm = new StateMachine("sm");
            var state1 = sm.CreateInitialState("state1");
            var evt = new Event<string>("evt");

            Assert.Throws<ArgumentNullException>(() => state1.TransitionOn(evt).To(null));

            var transition = state1.TransitionOn(evt).ToDynamic(i => null);
            Assert.Throws<ArgumentNullException>(() => transition.StateSelector = null);
        }
    }
}
