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
    public class StateSubclassTests
    {
        private class SubclassWithBadT : StateBase<State>
        {
        }

        private class SimpleSubclass : StateBase<SimpleSubclass>
        {
        }

        private class SubclassWithName : State
        {
            public SubclassWithName()
            {
                this.Name = "My Name";
            }
        }

        private class StateSubclassWithMethods : State
        {
            public IEvent CanTransitionEvent;
            public State CanTransitionTo;
            public object CanTransitionEventData;
            public bool CanTransitionResult = true;

            protected override bool CanTransition(IEvent @event, State to, object eventData)
            {
                this.CanTransitionEvent = @event;
                this.CanTransitionTo = to;
                this.CanTransitionEventData = eventData;
                return this.CanTransitionResult;
            }

            public IEvent HandleEventEvent;
            public object HandleEventEventData;
            public State HandleEventResult;

            protected override State HandleEvent(IEvent @event, object eventData)
            {
                this.HandleEventEvent = @event;
                this.HandleEventEventData = eventData;
                return this.HandleEventResult;
            }

            public StateHandlerInfo<State> OnEntryInfo;

            protected override void OnEntry(StateHandlerInfo<State> info)
            {
                this.OnEntryInfo = info;
            }

            public StateHandlerInfo<State> OnExitInfo;

            protected override void OnExit(StateHandlerInfo<State> info)
            {
                this.OnExitInfo = info;
            }
        }

        [Test]
        public void StateBaseThrowsIfSubclassDoesNotProvideoCorrectT()
        {
            Assert.Throws<ArgumentException>(() => new SubclassWithBadT());
        }

        [Test]
        public void ThrowsIfStateIsNotCreatedFromStateMachine()
        {
            var sm = new StateMachine<SimpleSubclass>();
            var state1 = sm.CreateInitialState("state1");
            var state2 = new SimpleSubclass();
            var evt = new Event("evt");

            Assert.Throws<InvalidOperationException>(() => state1.TransitionOn(evt).To(state2));
        }

        [Test]
        public void StateSubclassIsAllowedToProviedItsOwnName()
        {
            var sm = new StateMachine();
            var state1 = sm.CreateInitialState<SubclassWithName>();

            Assert.AreEqual("My Name", state1.Name);
        }

        [Test]
        public void StateSubclasOwnNameCanBeOverridden()
        {
            var sm = new StateMachine();
            var state1 = sm.CreateInitialState<SubclassWithName>("My Other Name");

            Assert.AreEqual("My Other Name", state1.Name);
        }

        [Test]
        public void CanTransitionReceivesCorrectData()
        {
            var sm = new StateMachine();
            var state1 = sm.CreateInitialState<StateSubclassWithMethods>("state1");
            var state2 = sm.CreateState("state2");
            var evt = new Event("evt");
            state1.TransitionOn(evt).To(state2);

            evt.Fire();

            Assert.AreEqual(state2, state1.CanTransitionTo);
            Assert.AreEqual(evt, state1.CanTransitionEvent);
            Assert.Null(state1.CanTransitionEventData);
        }

        [Test]
        public void CanTransitionReceivesCorrectDataForDynamicTransition()
        {
            var sm = new StateMachine();
            var state1 = sm.CreateInitialState<StateSubclassWithMethods>("state1");
            var state2 = sm.CreateState("state2");
            var evt = new Event("evt");
            state1.TransitionOn(evt).ToDynamic(i => state2);

            evt.Fire();

            Assert.AreEqual(state2, state1.CanTransitionTo);
            Assert.AreEqual(evt, state1.CanTransitionEvent);
            Assert.Null(state1.CanTransitionEventData);
        }

        [Test]
        public void CanTransitionReceivesCorrectDataForEventWithEventData()
        {
            var sm = new StateMachine();
            var state1 = sm.CreateInitialState<StateSubclassWithMethods>("state1");
            var state2 = sm.CreateState("state2");
            var evt = new Event<string>("evt");
            state1.TransitionOn(evt).To(state2);

            evt.Fire("foo");

            Assert.AreEqual(state2, state1.CanTransitionTo);
            Assert.AreEqual(evt, state1.CanTransitionEvent);
            Assert.AreEqual("foo", state1.CanTransitionEventData);
        }

        [Test]
        public void CanTransitionReceivesCorrectDataForDynamicTransitionAndEventWithEventData()
        {
            var sm = new StateMachine();
            var state1 = sm.CreateInitialState<StateSubclassWithMethods>("state1");
            var state2 = sm.CreateState("state2");
            var evt = new Event<string>("evt");
            state1.TransitionOn(evt).ToDynamic(i => state2);

            evt.Fire("foo");

            Assert.AreEqual(state2, state1.CanTransitionTo);
            Assert.AreEqual(evt, state1.CanTransitionEvent);
            Assert.AreEqual("foo", state1.CanTransitionEventData);
        }

        [Test]
        public void CanTransitionCanAbortTransition()
        {
            var sm = new StateMachine();
            var state1 = sm.CreateInitialState<StateSubclassWithMethods>("state1");
            state1.CanTransitionResult = false;
            var state2 = sm.CreateState("state2");
            var evt = new Event("evt");
            state1.TransitionOn(evt).To(state2);

            Assert.False(evt.TryFire());
        }

        [Test]
        public void CanTransitionCanAbortDynamicTransition()
        {
            var sm = new StateMachine();
            var state1 = sm.CreateInitialState<StateSubclassWithMethods>("state1");
            state1.CanTransitionResult = false;
            var state2 = sm.CreateState("state2");
            var evt = new Event("evt");
            state1.TransitionOn(evt).ToDynamic(i => state2);

            Assert.False(evt.TryFire());
        }

        [Test]
        public void CanTransitionCanAbortTransitionWithEventData()
        {
            var sm = new StateMachine();
            var state1 = sm.CreateInitialState<StateSubclassWithMethods>("state1");
            state1.CanTransitionResult = false;
            var state2 = sm.CreateState("state2");
            var evt = new Event<string>("evt");
            state1.TransitionOn(evt).To(state2);

            Assert.False(evt.TryFire("foo"));
        }

        [Test]
        public void CanTransitionCanAbortDynamicTransitionWithEventData()
        {
            var sm = new StateMachine();
            var state1 = sm.CreateInitialState<StateSubclassWithMethods>("state1");
            state1.CanTransitionResult = false;
            var state2 = sm.CreateState("state2");
            var evt = new Event<string>("evt");
            state1.TransitionOn(evt).ToDynamic(i => state2);

            Assert.False(evt.TryFire("foo"));
        }

        [Test]
        public void HandleEventReceivesCorrectData()
        {
            var sm = new StateMachine();
            var state1 = sm.CreateInitialState<StateSubclassWithMethods>("state1");
            var state2 = sm.CreateState("state2");
            var evt = new Event("evt");
            state1.TransitionOn(evt).To(state2);

            evt.Fire();

            Assert.AreEqual(evt, state1.HandleEventEvent);
            Assert.Null(state1.HandleEventEventData);
        }
        
        [Test]
        public void HandleEventCanChangeDestinationState()
        {
            var sm = new StateMachine();
            var state1 = sm.CreateInitialState<StateSubclassWithMethods>("state1");
            state1.HandleEventResult = state1;
            var state2 = sm.CreateState("state2");
            var evt = new Event("evt");
            state1.TransitionOn(evt).To(state2);

            evt.Fire();

            Assert.AreEqual(state1, sm.CurrentState);
        }

        [Test]
        public void HandleEventCalledEvenIfNoTransitionsOnThatEvent()
        {
            var sm = new StateMachine();
            var state1 = sm.CreateInitialState<StateSubclassWithMethods>("state1");
            state1.HandleEventResult = state1;
            var state2 = sm.CreateState("state2");
            var evt = new Event("evt");
            state2.TransitionOn(evt).To(state2);

            evt.Fire();

            Assert.AreEqual(evt, state1.HandleEventEvent);
            Assert.Null(state1.HandleEventEventData);
        }

        [Test]
        public void OnEntryReceivesCorrectData()
        {
            var sm = new StateMachine();
            var state1 = sm.CreateInitialState("state1");
            var state2 = sm.CreateState<StateSubclassWithMethods>("state2");
            var evt = new Event("evt");
            state1.TransitionOn(evt).To(state2);

            evt.Fire();

            var info = state2.OnEntryInfo;
            Assert.NotNull(info);
            Assert.AreEqual(state1, info.From);
            Assert.AreEqual(state2, info.To);
            Assert.AreEqual(evt, info.Event);
            Assert.False(info.IsInnerTransition);
            Assert.AreEqual(EventFireMethod.Fire, info.EventFireMethod);
        }

        [Test]
        public void OnExitReceivesCorrectData()
        {
            var sm = new StateMachine();
            var state1 = sm.CreateInitialState<StateSubclassWithMethods>("state1");
            var state2 = sm.CreateState("state2");
            var evt = new Event("evt");
            state1.TransitionOn(evt).To(state2);

            evt.TryFire();

            var info = state1.OnExitInfo;
            Assert.NotNull(info);
            Assert.AreEqual(state1, info.From);
            Assert.AreEqual(state2, info.To);
            Assert.AreEqual(evt, info.Event);
            Assert.False(info.IsInnerTransition);
            Assert.AreEqual(EventFireMethod.TryFire, info.EventFireMethod);
        }

        [Test]
        public void DoesNotThrowIfHandleEventReturnAStateFromADifferentStateMachine()
        {
            var sm = new StateMachine();
            var state1 = sm.CreateInitialState("state1");
            var state2 = sm.CreateState("state2");
            var subSm = state1.CreateChildStateMachine("subSm");
            var state11 = subSm.CreateInitialState<StateSubclassWithMethods>("state11");
            state11.HandleEventResult = state2;
            var state12 = subSm.CreateState();

            var evt = new Event("evt");
            state11.TransitionOn(evt).To(state12);

            Assert.DoesNotThrow(() => evt.Fire());
            Assert.AreEqual(state2, sm.CurrentState);
        }
    }
}
