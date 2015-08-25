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
    public class StateGroupTests
    {
        [Test]
        public void IndicatesWhetherInState()
        {
            var sm = new StateMachine("State Machine");
            var state1 = sm.CreateInitialState("State 1");
            var state2 = sm.CreateState("State 2");
            var state3 = sm.CreateState("State 3");

            var evt = sm.CreateEvent("Event");
            var group1 = new StateGroup("Group 1");
            var group2 = new StateGroup("Group 2");

            state1.AddToGroup(group1);
            state2.AddToGroup(group2);
            state3.AddToGroup(group2);

            state1.TransitionOn(evt).To(state2);
            state2.TransitionOn(evt).To(state3);

            Assert.True(group1.IsCurrent);
            Assert.False(group2.IsCurrent);

            evt.Fire();

            Assert.False(group1.IsCurrent);
            Assert.True(group2.IsCurrent);

            evt.Fire();

            Assert.False(group1.IsCurrent);
            Assert.True(group2.IsCurrent);
        }

        [Test]
        public void IsCurrentIncludesChildStateMachines()
        {
            var sm = new StateMachine("State Machine");
            var state1 = sm.CreateInitialState("State 1");
            var subSm = state1.CreateChildStateMachine("Child State Machine");
            var state11 = subSm.CreateInitialState("State 1.1");
            var state12 = subSm.CreateState("State 1.2");

            var group = new StateGroup("Group");
            state1.AddToGroup(group);

            var evt = sm.CreateEvent("Event");
            state11.TransitionOn(evt).To(state12);

            Assert.True(group.IsCurrent);

            evt.Fire();

            Assert.True(group.IsCurrent);
        }

        [Test]
        public void FiresEntryHandlerWithCorrectArgumentsWhenEntered()
        {
            var sm = new StateMachine("State Machine");
            var state1 = sm.CreateInitialState("State 1");
            var state2 = sm.CreateState("State 2");
            var evt = sm.CreateEvent("Event");
            StateHandlerInfo<State> info = null;
            var group = new StateGroup("Group")
                .WithEntry(i => info = i);
            state2.AddToGroup(group);
            state1.TransitionOn(evt).To(state2);

            evt.Fire();

            Assert.NotNull(info);
            Assert.AreEqual(state1, info.From);
            Assert.AreEqual(state2, info.To);
            Assert.AreEqual(evt, info.Event);
        }

        [Test]
        public void FiresExitHandlerWithCorrectArgumentsWhenExited()
        {
            var sm = new StateMachine("State Machine");
            var state1 = sm.CreateInitialState("State 1");
            var state2 = sm.CreateState("State 2");
            var evt = sm.CreateEvent("Event");
            StateHandlerInfo<State> info = null;
            var group = new StateGroup("Group")
                .WithExit(i => info = i);
            state1.AddToGroup(group);
            state1.TransitionOn(evt).To(state2);

            evt.Fire();

            Assert.NotNull(info);
            Assert.AreEqual(state1, info.From);
            Assert.AreEqual(state2, info.To);
            Assert.AreEqual(evt, info.Event);
        }

        [Test]
        public void DoesNotFireHandlersWhenTransitioningBetweenTwoStatesInGroup()
        {
            var sm = new StateMachine("State Machine");
            var state1 = sm.CreateInitialState("State 1");
            var state2 = sm.CreateState("State 2");
            var evt = sm.CreateEvent("Event");
            bool fired = false;
            var group = new StateGroup("Group")
                .WithEntry(i => fired = true)
                .WithExit(i => fired = true);
            state1.AddToGroup(group);
            state2.AddToGroup(group);
            state1.TransitionOn(evt).To(state2);

            evt.Fire();

            Assert.False(fired);
        }

        [Test]
        public void StateGroupListsStatesCorerctly()
        {
            var sm = new StateMachine("State Machine");
            var state1 = sm.CreateInitialState("State 1");
            var state2 = sm.CreateState("State 2");
            var group = new StateGroup("Group");
            state1.AddToGroup(group);
            state2.AddToGroup(group);

            Assert.That(group.States, Is.EqualTo(new[] { state1, state2 }));
        }

        [Test]
        public void StateListsStateGroupsCorrectly()
        {
            var sm = new StateMachine("State Machine");
            var state1 = sm.CreateInitialState("State 1");
            var group1 = new StateGroup("Group 1");
            var group2 = new StateGroup("Group 2");
            state1.AddToGroup(group1);
            state1.AddToGroup(group2);

            Assert.That(state1.Groups, Is.EquivalentTo(new[] { group1, group2 }));
        }

        [Test]
        public void EventInEntryHandlerPropagatedCorrectly()
        {
            var sm = new StateMachine("State Machine");
            var state1 = sm.CreateInitialState("State 1");
            var state2 = sm.CreateState("State 2");
            var evt = sm.CreateEvent("Event");
            state1.TransitionOn(evt).To(state2);

            var ex = new Exception("Foo");
            var group = new StateGroup("Group")
                .WithEntry(i => { throw ex; });
            state2.AddToGroup(group);

            var e = Assert.Throws<TransitionFailedException>(() => evt.Fire());
            Assert.AreEqual(state1, e.FaultInfo.From);
            Assert.AreEqual(state2, e.FaultInfo.To);
            Assert.AreEqual(evt, e.FaultInfo.Event);
            Assert.AreEqual(ex, e.FaultInfo.Exception);
            Assert.AreEqual(sm, e.FaultInfo.StateMachine);
            Assert.AreEqual(FaultedComponent.GroupEntryHandler, e.FaultInfo.FaultedComponent);
            Assert.AreEqual(group, e.FaultInfo.Group);
        }

        [Test]
        public void EventInExitHandlerPropagatedCorrectly()
        {
            var sm = new StateMachine("State Machine");
            var state1 = sm.CreateInitialState("State 1");
            var state2 = sm.CreateState("State 2");
            var evt = sm.CreateEvent("Event");
            state1.TransitionOn(evt).To(state2);

            var ex = new Exception("Foo");
            var group = new StateGroup("Group")
                .WithExit(i => { throw ex; });
            state1.AddToGroup(group);

            var e = Assert.Throws<TransitionFailedException>(() => evt.Fire());
            Assert.AreEqual(state1, e.FaultInfo.From);
            Assert.AreEqual(state2, e.FaultInfo.To);
            Assert.AreEqual(evt, e.FaultInfo.Event);
            Assert.AreEqual(ex, e.FaultInfo.Exception);
            Assert.AreEqual(sm, e.FaultInfo.StateMachine);
            Assert.AreEqual(FaultedComponent.GroupExitHandler, e.FaultInfo.FaultedComponent);
            Assert.AreEqual(group, e.FaultInfo.Group);
        }
    }
}
