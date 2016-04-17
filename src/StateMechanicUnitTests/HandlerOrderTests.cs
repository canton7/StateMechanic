using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using StateMechanic;
using System;

namespace StateMechanicUnitTests
{
    [TestFixture]
    public class HandlerOrderTests
    {
        [Test]
        public void CorrectHandlersAreInvokedInNormalTransition()
        {
            var events = new List<string>();
            var sm = new StateMachine("State Machine");
            var evt = new Event("Event");
            var state1 = sm.CreateInitialState("State 1")
                .WithEntry(i => events.Add("State 1 Entry"))
                .WithExit(i => events.Add("State 1 Exit"));
            var state2 = sm.CreateState("State 2")
                .WithEntry(i => events.Add("State 2 Entry"))
                .WithExit(i => events.Add("State 2 Exit"));
            var transition = state1.TransitionOn(evt).To(state2).WithHandler(i => events.Add("Transition 1 2"));

            evt.Fire();

            Assert.That(events, Is.EquivalentTo(new[] { "State 1 Exit", "Transition 1 2", "State 2 Entry" }));
        }

        [Test]
        public void CorrectHandlersAreInvokedInDynamicTransition()
        {
            var events = new List<string>();
            var sm = new StateMachine("State Machine");
            var evt = new Event("Event");
            var state1 = sm.CreateInitialState("State 1").WithExit(i => events.Add("State 1 Exit"));
            var state2 = sm.CreateState("State 2").WithEntry(i => events.Add("State 2 Entry"));
            var transition = state1.TransitionOn(evt).ToDynamic(i =>
            {
                events.Add("Dynamic Handler");
                return state2;
            }).WithHandler(i => events.Add("Transition 1 2"));

            evt.Fire();

            Assert.That(events, Is.EquivalentTo(new[] { "Dynamic Handler", "State 1 Exit", "Transition 1 2", "State 2 Entry" }));
        }

        [Test]
        public void NormalSelfTransitionShouldFireExitAndEntry()
        {
            var events = new List<string>();
            var sm = new StateMachine("State Machine");
            var evt = new Event("Event");
            var state1 = sm.CreateInitialState("State 1").WithEntry(i => events.Add("State 1 Entry")).WithExit(i => events.Add("State 1 Exit"));
            state1.TransitionOn(evt).To(state1).WithHandler(i => events.Add("Transition 1 1"));

            evt.Fire();

            Assert.That(events, Is.EquivalentTo(new[] { "State 1 Exit", "Transition 1 1", "State 1 Entry" }));
        }

        [Test]
        public void InnerSelfTransitionShouldNotFireExitAndEntry()
        {
            var events = new List<string>();
            var sm = new StateMachine("State Machine");
            var evt = new Event("Event");
            var state1 = sm.CreateInitialState("State 1").WithEntry(i => events.Add("State 1 Entry")).WithExit(i => events.Add("State 1 Exit"));
            state1.InnerSelfTransitionOn(evt).WithHandler(i => events.Add("Transition 1 1 Inner"));

            evt.Fire();

            Assert.That(events, Is.EquivalentTo(new[] { "Transition 1 1 Inner" }));
        }

        [Test]
        public void InnerSelfTransitionOnEventTShouldNotFireExitAndEntry()
        {
            var events = new List<string>();
            var sm = new StateMachine("State Machine");
            var evt = new Event<int>("Event");
            var state1 = sm.CreateInitialState("State 1").WithEntry(i => events.Add("State 1 Entry")).WithExit(i => events.Add("State 1 Exit"));
            state1.InnerSelfTransitionOn(evt).WithHandler(i => events.Add("Transition 1 1 Inner"));

            evt.Fire(3);

            Assert.That(events, Is.EquivalentTo(new[] { "Transition 1 1 Inner" }));
        }

        [Test]
        public void TransitioningToChildStateMachineCallsEntryHandlerOnInitialState()
        {
            StateHandlerInfo<State> state21EntryInfo = null;

            var sm = new StateMachine("State Machine");
            var evt = new Event("Event");
            var state1 = sm.CreateInitialState("State 1");
            var state2 = sm.CreateState("State 2");
            var subSm = state2.CreateChildStateMachine();
            var state21 = subSm.CreateInitialState("State 2.1").WithEntry(i => state21EntryInfo = i);
            state1.TransitionOn(evt).To(state2);

            evt.Fire();

            Assert.NotNull(state21EntryInfo);
            Assert.AreEqual(state1, state21EntryInfo.From);
            Assert.AreEqual(state21, state21EntryInfo.To);
            Assert.AreEqual(evt, state21EntryInfo.Event);
        }

        [Test]
        public void TransitioningToTwoChildStateMachinesCallsEntryHandlerOnInitialState()
        {
            StateHandlerInfo<State> state211EntryInfo = null;

            var sm = new StateMachine("State Machine");
            var evt = new Event("Event");
            var state1 = sm.CreateInitialState("State 1");
            var state2 = sm.CreateState("State 2");
            var subSm = state2.CreateChildStateMachine();
            var state21 = subSm.CreateInitialState("State 2.1");
            var subSubSm = state21.CreateChildStateMachine("subSubSm");
            var state211 = subSubSm.CreateInitialState("State 2.1.1").WithEntry(i => state211EntryInfo = i);
            state1.TransitionOn(evt).To(state2);

            evt.Fire();

            Assert.NotNull(state211EntryInfo);
            Assert.AreEqual(state1, state211EntryInfo.From);
            Assert.AreEqual(state211, state211EntryInfo.To);
            Assert.AreEqual(evt, state211EntryInfo.Event);
        }

        [Test]
        public void TransitioningFromChildStateMachineCallsExitHandlerOnCurrentState()
        {
            StateHandlerInfo<State> state22ExitInfo = null;

            var sm = new StateMachine("State Machine");
            var evt1 = new Event("Event 1");
            var evt2 = new Event("Event 2");
            var state1 = sm.CreateInitialState("State 1");
            var state2 = sm.CreateState("State 2");
            var subSm = state2.CreateChildStateMachine();
            var state21 = subSm.CreateInitialState("State 2.1");
            var state22 = subSm.CreateState("State 2.2").WithExit(i => state22ExitInfo = i);

            state1.TransitionOn(evt1).To(state2);
            state2.TransitionOn(evt2).To(state1);
            state21.TransitionOn(evt1).To(state22);

            // Enter state2, and start child state machine
            evt1.Fire();
            // Enter State 2.2
            evt1.Fire();
            // Transition from state2 to state1, exiting the child state machine
            evt2.Fire();

            Assert.NotNull(state22ExitInfo);
            Assert.AreEqual(state22, state22ExitInfo.From);
            Assert.AreEqual(state1, state22ExitInfo.To);
            Assert.AreEqual(evt2, state22ExitInfo.Event);
        }

        [Test]
        public void TransitioningFromTwoChildStateMachinesCallsExitHandlerOnCurrentState()
        {
            StateHandlerInfo<State> state212ExitInfo = null;

            var sm = new StateMachine("State Machine");
            var evt1 = new Event("Event 1");
            var evt2 = new Event("Event 2");
            var state1 = sm.CreateInitialState("State 1");
            var state2 = sm.CreateState("State 2");
            var subSm = state2.CreateChildStateMachine();
            var state21 = subSm.CreateInitialState("State 2.1");
            var subSubSm = state21.CreateChildStateMachine();
            var state211 = subSubSm.CreateInitialState("State 2.1.1");
            var state212 = subSubSm.CreateState("State 2.1.2").WithExit(i => state212ExitInfo = i);

            state1.TransitionOn(evt1).To(state2);
            state2.TransitionOn(evt2).To(state1);
            state211.TransitionOn(evt1).To(state212);

            // Enter State 2 and State 2.1 and State 2.1.1, and start child state machine
            evt1.Fire();
            // Enter State 1.2.1
            evt1.Fire();
            // Transition from state2 to state1, exiting the child state machine
            evt2.Fire();

            Assert.NotNull(state212ExitInfo);
            Assert.AreEqual(state212, state212ExitInfo.From);
            Assert.AreEqual(state1, state212ExitInfo.To);
            Assert.AreEqual(evt2, state212ExitInfo.Event);
        }

        [Test]
        public void ForcefullyTransitioningToChildStateMachineCallsEntryHandlerOnInitialState()
        {
            StateHandlerInfo<State> state21EntryInfo = null;

            var sm = new StateMachine("State Machine");
            var evt = new Event("Event");
            var state1 = sm.CreateInitialState("State 1");
            var state2 = sm.CreateState("State 2");
            var subSm = state2.CreateChildStateMachine();
            var state21 = subSm.CreateInitialState("State 2.1").WithEntry(i => state21EntryInfo = i);

            sm.ForceTransition(state2, evt);

            Assert.NotNull(state21EntryInfo);
            Assert.AreEqual(state1, state21EntryInfo.From);
            Assert.AreEqual(state21, state21EntryInfo.To);
            Assert.AreEqual(evt, state21EntryInfo.Event);
        }

        [Test]
        public void ForcefullyTransitioningFromChildStateMachineCallsExitHandlerOnCurrentState()
        {
            StateHandlerInfo<State> state22ExitInfo = null;

            var sm = new StateMachine("State Machine");
            var evt1 = new Event("Event 1");
            var state1 = sm.CreateInitialState("State 1");
            var state2 = sm.CreateState("State 2");
            var subSm = state2.CreateChildStateMachine();
            var state21 = subSm.CreateInitialState("State 2.1");
            var state22 = subSm.CreateState("State 2.2").WithExit(i => state22ExitInfo = i);

            state1.TransitionOn(evt1).To(state2);
            state21.TransitionOn(evt1).To(state22);

            // Enter state2, and start child state machine
            evt1.Fire();
            // Enter State 2.2
            evt1.Fire();
            // Transition from state2 to state1, exiting the child state machine
            sm.ForceTransition(state1, evt1);

            Assert.NotNull(state22ExitInfo);
            Assert.AreEqual(state22, state22ExitInfo.From);
            Assert.AreEqual(state1, state22ExitInfo.To);
            Assert.AreEqual(evt1, state22ExitInfo.Event);
        }

        [Test]
        public void ForcefullyTransitioningToMoreChildStateCallsCorrectHandlers()
        {
            var log = new List<Tuple<string, StateHandlerInfo<State>>>();

            var sm = new StateMachine("sm");
            var state1 = sm.CreateInitialState("state1");

            var childSm1 = state1.CreateChildStateMachine("childSm1");
            var state11 = childSm1.CreateInitialState("state11");
            var state12 = childSm1.CreateState("state12");

            var childSm11 = state11.CreateChildStateMachine("childsm11");
            var state111 = childSm11.CreateInitialState("state111");
            var state112 = childSm11.CreateState("state112");

            var childSm12 = state12.CreateChildStateMachine("childSm12");
            var state121 = childSm12.CreateInitialState("state121");
            var state122 = childSm12.CreateState("state122");

            var evt = new Event("evt");

            // Start off in state112, forcefully transition to state122
            sm.ForceTransition(state112, evt);

            state1.EntryHandler = x => log.Add(Tuple.Create("state1 Entry", x));
            state1.ExitHandler = x => log.Add(Tuple.Create("state1 Exit", x));

            state11.EntryHandler = x => log.Add(Tuple.Create("state11 Entry", x));
            state11.ExitHandler = x => log.Add(Tuple.Create("state11 Exit", x));

            state111.EntryHandler = x => log.Add(Tuple.Create("state111 Entry", x));
            state111.ExitHandler = x => log.Add(Tuple.Create("state111 Exit", x));

            state112.EntryHandler = x => log.Add(Tuple.Create("state112 Entry", x));
            state112.ExitHandler = x => log.Add(Tuple.Create("state112 Exit", x));

            state12.EntryHandler = x => log.Add(Tuple.Create("state12 Entry", x));
            state12.ExitHandler = x => log.Add(Tuple.Create("state12 Exit", x));

            state121.EntryHandler = x => log.Add(Tuple.Create("state121 Entry", x));
            state121.ExitHandler = x => log.Add(Tuple.Create("state121 Exit", x));

            state122.EntryHandler = x => log.Add(Tuple.Create("state122 Entry", x));
            state122.ExitHandler = x => log.Add(Tuple.Create("state122 Exit", x));

            sm.ForceTransition(state122, evt);

            Assert.AreEqual(6, log.Count);

            Assert.AreEqual("state112 Exit", log[0].Item1);
            Assert.AreEqual(evt, log[0].Item2.Event);
            Assert.AreEqual(state112, log[0].Item2.From);
            Assert.AreEqual(state12, log[0].Item2.To);

            Assert.AreEqual("state11 Exit", log[1].Item1);
            Assert.AreEqual(evt, log[1].Item2.Event);
            Assert.AreEqual(state11, log[1].Item2.From);
            Assert.AreEqual(state12, log[1].Item2.To);

            Assert.AreEqual("state12 Entry", log[2].Item1);
            Assert.AreEqual(evt, log[2].Item2.Event);
            Assert.AreEqual(state11, log[2].Item2.From);
            Assert.AreEqual(state12, log[2].Item2.To);

            Assert.AreEqual("state121 Entry", log[3].Item1);
            Assert.AreEqual(evt, log[3].Item2.Event);
            Assert.AreEqual(state11, log[3].Item2.From);
            Assert.AreEqual(state121, log[3].Item2.To);

            Assert.AreEqual("state121 Exit", log[4].Item1);
            Assert.AreEqual(evt, log[4].Item2.Event);
            Assert.AreEqual(state121, log[4].Item2.From);
            Assert.AreEqual(state122, log[4].Item2.To);

            Assert.AreEqual("state122 Entry", log[5].Item1);
            Assert.AreEqual(evt, log[5].Item2.Event);
            Assert.AreEqual(state121, log[5].Item2.From);
            Assert.AreEqual(state122, log[5].Item2.To);
        }
    }
}
