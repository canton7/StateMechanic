using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StateMechanic;

namespace Samples
{
    /// <summary>
    /// Examples from the README
    /// </summary>
    public static class Readme
    {
        [Description("Quick Start")]
        public static void QuickStart()
        {
            // First off, create a state machine. It's recommended to give it a name
            // (which helps debugging), but you don't have to.
            var stateMachine = new StateMachine("State Machine");

            // Whenever we create a state machine, we need to give it an initial state. This
            // is the state which the state machine starts in.
            var awaitingMoney = stateMachine.CreateInitialState("Awaiting Money");

            // We then add the other states in the staate machine.
            var awaitingSelection = stateMachine.CreateState("Awaiting Selection");
            var dispensing = stateMachine.CreateState("Dispensing");

            // Next, we'll create the events.
            var moneyReceived = new Event("Money Received");
            var selectionReceived = new Event("Selection Received");
            var dispenseComplete = new Event("Dispense Complete");

            // Finally, we create transitions between our states based on these events.
            awaitingMoney.TransitionOn(moneyReceived).To(awaitingSelection);
            awaitingSelection.TransitionOn(selectionReceived).To(dispensing);
            dispensing.TransitionOn(dispenseComplete).To(awaitingMoney);

            // See that the state machine starts in the initial state
            Assert.AreEqual(awaitingMoney, stateMachine.CurrentState);
            Assert.True(awaitingMoney.IsCurrent);

            // Fire events directly. This will throw an exception if there is no transition
            // from the current state.
            moneyReceived.Fire();

            // Alternatively, you can try and fire the event - this won't throw on failure,
            // but will return a bool indicating whether it succeeded.
            moneyReceived.TryFire();

            Assert.AreEqual(awaitingSelection, stateMachine.CurrentState);
        }

        [Description("State Entry/Exit Handlers")]
        public static void EntryExitHandlers()
        {
            var stateMachine = new StateMachine();
            var initial = stateMachine.CreateInitialState("Initial");

            ///////////////////////

            var someState = stateMachine.CreateState("Some State")
                .WithEntry(info => Console.WriteLine($"Entry from {info.From} to {info.To} on {info.Event}"))
                .WithExit(info => Console.WriteLine($"Exit from {info.From} to {info.To} on {info.Event}"));

            // You can also set the EntryHandler and ExitHandler properties directly
            someState.EntryHandler = info => Console.WriteLine($"Entry from {info.From} to {info.To} on {info.Event}");
            someState.ExitHandler = info => Console.WriteLine($"Exit from {info.From} to {info.To} on {info.Event}");

            ///////////////////////

            var evt = new Event("event");
            initial.TransitionOn(evt).To(someState);
            someState.TransitionOn(evt).To(initial);

            evt.Fire();
            evt.Fire();
        }

        [Description("Transition Handlers")]
        public static void TransitionHandlers()
        {
            var stateMachine = new StateMachine();
            var someState = stateMachine.CreateInitialState("Some State");
            var someOtherState = stateMachine.CreateState("Some Other State");
            var someEvent = new Event("Some Event");

            ///////////////////////

            someState.TransitionOn(someEvent).To(someOtherState)
                .WithHandler(info => Console.WriteLine($"Transition from {info.From} to {info.To} on {info.Event}"));

            // You can also set the Handler property directly
            var transition = someState.TransitionOn(someEvent).To(someOtherState);
            transition.Handler = info => Console.WriteLine($"Transition from {info.From} to {info.To} on {info.Event}");

            ///////////////////////

            someEvent.Fire();
        }

        [Description("Inner Self Transitions")]
        public static void InnerSelfTransitions()
        {
            var stateMachine = new StateMachine();

            var event1 = new Event("Event 1");
            var event2 = new Event("Event 2");

            ///////////////////////

            var state = stateMachine.CreateInitialState("State")
                .WithEntry(i => Console.WriteLine("Entry"))
                .WithExit(i => Console.WriteLine("Exit"));

            state.TransitionOn(event1).To(state).WithHandler(i => Console.WriteLine("Handler"));

            state.InnerSelfTransitionOn(event2).WithHandler(i => Console.WriteLine("Handler"));

            event1.Fire();
            // Prints: Exit, Handler, Entry

            event2.Fire();
            // Prints: Handler

            ///////////////////////
        }

        [Description("Event Data")]
        public static void EventData()
        {
            var stateMachine = new StateMachine();
            var state = stateMachine.CreateInitialState("Initial State");
            var anotherState = stateMachine.CreateState("Another State");

            ///////////////////////

            // This is an event which takes a string argument (but you can use any data type)
            var eventWithData = new Event<string>();

            state.TransitionOn(eventWithData).To(anotherState)
                .WithHandler(info => Console.WriteLine($"Data: {info.EventData}"));

            // Provide the data when you fire the event
            eventWithData.Fire("Some Data");

            // Prints: "Data: Some Data"

            ///////////////////////
        }

        [Description("Transition Guards")]
        public static void TransitionGuards()
        {
            var stateMachine = new StateMachine();
            var stateA = stateMachine.CreateInitialState("State A");
            var stateB = stateMachine.CreateState("State B");
            var stateC = stateMachine.CreateState("State C");
            var eventE = new Event("Event E");

            ///////////////////////

            bool allowTransitionToStateB = false;

            stateA.TransitionOn(eventE).To(stateB).WithGuard(info => allowTransitionToStateB);
            stateA.TransitionOn(eventE).To(stateC);

            eventE.Fire();
            Assert.AreEqual(stateC, stateMachine.CurrentState);

            ///////////////////////

            stateMachine.Reset();

            ///////////////////////

            // Alternatively...
            allowTransitionToStateB = true;
            eventE.Fire();
            Assert.AreEqual(stateB, stateMachine.CurrentState);

            ///////////////////////
        }

        [Description("Dynamic Transitions")]
        public static void DynamicTransitions()
        {
            var stateMachine = new StateMachine();
            var stateA = stateMachine.CreateInitialState("State A");
            var stateB = stateMachine.CreateState("State B");
            var stateC = stateMachine.CreateState("State C");
            var eventE = new Event("Event E");

            ///////////////////////

            State stateToTransitionTo = stateB;

            stateA.TransitionOn(eventE).ToDynamic(info => stateToTransitionTo);

            eventE.Fire();
            Assert.AreEqual(stateB, stateMachine.CurrentState);

            ///////////////////////

            stateMachine.Reset();

            ///////////////////////

            // Alternatively...
            stateToTransitionTo = stateC;
            eventE.Fire();
            Assert.AreEqual(stateC, stateMachine.CurrentState);
        }

        [Description("State Groups")]
        public static void StateGroups()
        {
            var stateMachine = new StateMachine();
            var initial = stateMachine.CreateInitialState("Initial");
            var evt = new Event("evt");

            ///////////////////////

            var stateA = stateMachine.CreateState("State A");
            var stateB = stateMachine.CreateState("State B");

            // You can create state groups, and add states to them
            var statesAAndB = new StateGroup("States A and B")
                .WithEntry(info => Console.WriteLine($"Entering group from {info.From} to {info.To} on {info.Event}"))
                .WithExit(info => Console.WriteLine($"Exiting group from {info.From} to {info.To} on {info.Event}"));

            statesAAndB.AddStates(stateA, stateB);

            // You can also add states to groups
            stateA.AddToGroup(statesAAndB);

            ///////////////////////

            initial.TransitionOn(evt).To(stateA);
            stateA.TransitionOn(evt).To(stateB);
            stateB.TransitionOn(evt).To(initial);

            evt.Fire();
            evt.Fire();
            evt.Fire();
        }
    }
}
