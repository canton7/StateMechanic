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
    public class TransitionInfoTests
    {
        private struct EventData
        {
            public int Foo { get; set; }
        }

        [Test]
        public void CorrectInfoIsGivenInGuard()
        {
            TransitionInfo<State> guardInfo = null;

            var sm = new StateMachine("State Machine");
            var evt = new Event("Event");
            var state1 = sm.CreateInitialState("State 1");
            var state2 = sm.CreateState("State 2");
            state1.TransitionOn(evt).To(state2).WithGuard(i => { guardInfo = i; return true; });

            evt.Fire();

            Assert.NotNull(guardInfo);
            Assert.AreEqual(state1, guardInfo.From);
            Assert.AreEqual(state2, guardInfo.To);
            Assert.AreEqual(evt, guardInfo.Event);
            Assert.False(guardInfo.IsInnerTransition);
        }

        [Test]
        public void CorrectInfoIsGivenInExitHandler()
        {
            StateHandlerInfo<State>? nullableHandlerInfo = null;

            var sm = new StateMachine("State Machine");
            var evt = new Event("Event");
            var state1 = sm.CreateInitialState("State 1").WithExit(i => nullableHandlerInfo = i);
            var state2 = sm.CreateState("State 2");
            state1.TransitionOn(evt).To(state2);

            evt.Fire();

            Assert.NotNull(nullableHandlerInfo);
            var handlerInfo = nullableHandlerInfo.Value;
            Assert.AreEqual(state1, handlerInfo.From);
            Assert.AreEqual(state2, handlerInfo.To);
            Assert.AreEqual(evt, handlerInfo.Event);
            Assert.Null(handlerInfo.EventData);
        }

        [Test]
        public void CorrectInfoIsGivenInExitHandlerT()
        {
            StateHandlerInfo<State>? nullableHandlerInfo = null;

            var sm = new StateMachine("State Machine");
            var evt = new Event<string>("Event");
            var state1 = sm.CreateInitialState("State 1").WithExit(i => nullableHandlerInfo = i);
            var state2 = sm.CreateState("State 2");
            state1.TransitionOn(evt).To(state2);

            evt.Fire("foo");

            Assert.NotNull(nullableHandlerInfo);
            var handlerInfo = nullableHandlerInfo.Value;
            Assert.AreEqual(state1, handlerInfo.From);
            Assert.AreEqual(state2, handlerInfo.To);
            Assert.AreEqual(evt, handlerInfo.Event);
            Assert.AreEqual("foo", handlerInfo.EventData);
        }

        [Test]
        public void CorrectInfoIsGivenInEntryHandler()
        {
            StateHandlerInfo<State>? nullableHandlerInfo = null;

            var sm = new StateMachine("State Machine");
            var evt = new Event("Event");
            var state1 = sm.CreateInitialState("State 1");
            var state2 = sm.CreateState("State 2").WithEntry(i => nullableHandlerInfo = i);
            state1.TransitionOn(evt).To(state2);

            evt.Fire();

            Assert.NotNull(nullableHandlerInfo);
            var handlerInfo = nullableHandlerInfo.Value;
            Assert.AreEqual(state1, handlerInfo.From);
            Assert.AreEqual(state2, handlerInfo.To);
            Assert.AreEqual(evt, handlerInfo.Event);
            Assert.False(handlerInfo.IsInnerTransition);
            Assert.Null(handlerInfo.EventData);
        }

        [Test]
        public void CorrectInfoIsGivenInEntryHandlerT()
        {
            StateHandlerInfo<State>? nullableHandlerInfo = null;

            var sm = new StateMachine("State Machine");
            var evt = new Event<int>("Event");
            var state1 = sm.CreateInitialState("State 1");
            var state2 = sm.CreateState("State 2").WithEntry(i => nullableHandlerInfo = i);
            state1.TransitionOn(evt).To(state2);

            evt.Fire(3);

            Assert.NotNull(nullableHandlerInfo);
            var handlerInfo = nullableHandlerInfo.Value;
            Assert.AreEqual(state1, handlerInfo.From);
            Assert.AreEqual(state2, handlerInfo.To);
            Assert.AreEqual(evt, handlerInfo.Event);
            Assert.False(handlerInfo.IsInnerTransition);
            Assert.AreEqual(3, handlerInfo.EventData);
        }

        [Test]
        public void CorrectInfoIsGivenInTransitionHandler()
        {
            TransitionInfo<State> transitionInfo = null;

            var sm = new StateMachine("State Machine");
            var evt = new Event("Event");
            var state1 = sm.CreateInitialState("State 1");
            var state2 = sm.CreateState("State 2");
            state1.TransitionOn(evt).To(state2).WithHandler(i => transitionInfo = i);

            evt.Fire();

            Assert.NotNull(transitionInfo);
            Assert.AreEqual(state1, transitionInfo.From);
            Assert.AreEqual(state2, transitionInfo.To);
            Assert.AreEqual(evt, transitionInfo.Event);
            Assert.False(transitionInfo.IsInnerTransition);
        }

        [Test]
        public void CorrectInfoIsGivenInTransitionHandlerOnInnerTransition()
        {
            TransitionInfo<State> transitionInfo = null;

            var sm = new StateMachine("State Machine");
            var evt = new Event("Event");
            var state1 = sm.CreateInitialState("State 1");
            state1.InnerSelfTransitionOn(evt).WithHandler(i => transitionInfo = i);

            evt.Fire();

            Assert.NotNull(transitionInfo);
            Assert.AreEqual(state1, transitionInfo.From);
            Assert.AreEqual(state1, transitionInfo.To);
            Assert.AreEqual(evt, transitionInfo.Event);
            Assert.True(transitionInfo.IsInnerTransition);
        }

        [Test]
        public void EventDataIsGivenToTransitionHandler()
        {
            EventData eventData = new EventData();

            var sm = new StateMachine("State Machine");
            var evt = new Event<EventData>("Event");
            var state1 = sm.CreateInitialState("State 1");
            var state2 = sm.CreateState("State 2");

            state1.TransitionOn(evt).To(state2).WithHandler(i => eventData = i.EventData);

            evt.Fire(new EventData() { Foo = 2 });

            Assert.AreEqual(2, eventData.Foo);
        }

        [Test]
        public void StateSelectorGetsCorrectInfo()
        {
            DynamicSelectorInfo<State>? nullableInfo = null;

            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var evt = new Event("evt");
            initial.TransitionOn(evt).ToDynamic(i =>
            {
                nullableInfo = i;
                return initial;
            });

            evt.Fire();

            Assert.NotNull(nullableInfo);
            var info = nullableInfo.Value;
            Assert.AreEqual(initial, info.From);
            Assert.AreEqual(evt, info.Event);
        }

        [Test]
        public void StateSelectorWithEventDataGetsCorrectInfo()
        {
            DynamicSelectorInfo<State, string>? nullableInfo = null;

            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var evt = new Event<string>("evt");
            initial.TransitionOn(evt).ToDynamic(i =>
            {
                nullableInfo = i;
                return initial;
            });

            evt.Fire("hello");

            Assert.NotNull(nullableInfo);
            var info = nullableInfo.Value;
            Assert.AreEqual(initial, info.From);
            Assert.AreEqual(evt, info.Event);
            Assert.AreEqual("hello", info.EventData);
        }

        [Test]
        public void DynamicTransitionHandlerGetsCorrectInfo()
        {
            TransitionInfo<State> info = null;

            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state1");
            var evt = new Event("evt");
            initial.TransitionOn(evt).ToDynamic(i => state1).WithHandler(i => info = i);

            evt.Fire();

            Assert.NotNull(info);
            Assert.AreEqual(initial, info.From);
            Assert.AreEqual(state1, info.To);
            Assert.AreEqual(evt, info.Event);
            Assert.False(info.IsInnerTransition);
        }

        [Test]
        public void DynamicTransitionHandlerWithEventDataGetsCorrectInfo()
        {
            TransitionInfo<State, string> info = null;

            var sm = new StateMachine("sm");
            var initial = sm.CreateInitialState("initial");
            var state1 = sm.CreateState("state1");
            var evt = new Event<string>("evt");
            initial.TransitionOn(evt).ToDynamic(i => state1).WithHandler(i => info = i);

            evt.Fire("foo");

            Assert.NotNull(info);
            Assert.AreEqual(initial, info.From);
            Assert.AreEqual(state1, info.To);
            Assert.AreEqual(evt, info.Event);
            Assert.False(info.IsInnerTransition);
            Assert.AreEqual("foo", info.EventData);
            Assert.AreEqual(EventFireMethod.Fire, info.EventFireMethod);
        }
    }
}
