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
    public static class ReadmeSamples
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

        [Description("Child State Machines")]
        public static void ChildStateMachines()
        {
            var parentStateMachine = new StateMachine("Parent");
            parentStateMachine.Transition += (o, e) => Console.WriteLine($"Transition: {e.From.Name} -> {e.To.Name} ({e.Event.Name})");

            var disconnected = parentStateMachine.CreateInitialState("Disconnected")
                .WithEntry(i => Console.WriteLine($"Disconnected entered: {i}"))
                .WithExit(i => Console.WriteLine($"Disconnected exited: {i}"));

            var connectingSuperState = parentStateMachine.CreateState("ConnectingSuperState")
                .WithEntry(i => Console.WriteLine($"ConnectingSuperState entered: {i}"))
                .WithExit(i => Console.WriteLine($"ConnectingSuperState exited: {i}"));

            var connectedSuperState = parentStateMachine.CreateState("ConnectedSuperState")
                .WithEntry(i => Console.WriteLine($"ConnectedSuperState entered: {i}"))
                .WithExit(i => Console.WriteLine($"ConnectedSuperState exited: {i}"));

            // 'Connecting' child state machine
            var connectingStateMachine = connectingSuperState.CreateChildStateMachine("ConnectingSM");

            var connectingInitialise = connectingStateMachine.CreateInitialState("ConnectingInitialisation")
                .WithEntry(i => Console.WriteLine($"ConnectingInitialisation entered: {i}"))
                .WithExit(i => Console.WriteLine($"ConnectingInitialisation exited: {i}"));

            var connecting = connectingStateMachine.CreateState("Connecting")
                .WithEntry(i => Console.WriteLine($"Connecting entered: {i}"))
                .WithExit(i => Console.WriteLine($"Connecting exited: {i}"));

            var handshaking = connectingStateMachine.CreateState("Handshaking")
                .WithEntry(i => Console.WriteLine($"Handshaking entered: {i}"))
                .WithExit(i => Console.WriteLine($"Handshaking exited: {i}"));

            // 'Connected' child state machine
            var connectedStateMachine = connectedSuperState.CreateChildStateMachine("ConnectedSM");

            var authorising = connectedStateMachine.CreateInitialState("Authorising")
                .WithEntry(i => Console.WriteLine($"Authorising entered: {i}"))
                .WithExit(i => Console.WriteLine($"Authorising exited: {i}"));

            var connected = connectedStateMachine.CreateState("Connected")
                .WithEntry(i => Console.WriteLine($"Connected entered: {i}"))
                .WithExit(i => Console.WriteLine($"Connected exited: {i}"));

            var eventConnect = new Event("Connect");
            var eventConnectionInitialised = new Event("Connecting Initialised");
            var eventConnected = new Event("Connected");
            var eventHandshakingCompleted = new Event("Handshaking Completed");
            var eventAuthorisingCompleted = new Event("Authorising Completed");

            var eventDisconnected = new Event("Disconnected");

            disconnected.TransitionOn(eventConnect).To(connectingSuperState)
                .WithHandler(i => Console.WriteLine($"Transition Handler: {i}"));

            connectingInitialise.TransitionOn(eventConnectionInitialised).To(connecting)
                .WithHandler(i => Console.WriteLine($"Transition Handler: {i}"));

            connecting.TransitionOn(eventConnected).To(handshaking)
                .WithHandler(i => Console.WriteLine($"Transition Handler: {i}"));

            handshaking.TransitionOn(eventDisconnected).To(connecting)
                .WithHandler(i => Console.WriteLine($"Transition Handler: {i}"));

            connectingSuperState.TransitionOn(eventHandshakingCompleted).To(connectedSuperState)
                .WithHandler(i => Console.WriteLine($"Transition Handler: {i}"));

            authorising.TransitionOn(eventAuthorisingCompleted).To(connected)
                .WithHandler(i => Console.WriteLine($"Transition Handler: {i}")); ;

            connectingSuperState.TransitionOn(eventDisconnected).To(disconnected)
                .WithHandler(i => Console.WriteLine($"Transition Handler: {i}"));

            connectedSuperState.TransitionOn(eventDisconnected).To(disconnected)
                .WithHandler(i => Console.WriteLine($"Transition Handler: {i}"));

            // This is a "successful" path through the system

            // We start off in the initial state
            Assert.AreEqual(disconnected, parentStateMachine.CurrentState);
            // And the child state machines are not active
            Assert.False(connectingStateMachine.IsActive);


            // This causes a transition from disconnected -> connectingSuperState. Since connectingSuperState
            // has a child state machine, this is activated and its initial state connectingInitialise is entered.
            // The following events therefore occur:
            // 1. Disconnected's Exit Handler is called. From=Disconnected, To=ConnectingSuperState
            // 2. The transition handler from Disconnected to ConnectingSuperState is called. From=Disconnected,
            //    To=ConnectingSuperState
            // 3. ConnectingSuperState's Entry Handler is called. From=Disconnected, To=ConnectingSuperState
            // 4. ConnectingInitialisation's Entry Handler is called. From=Disconnected, To=ConnectingInitialisation
            // 5. The Transition event on the parentStateMachine is raised
            eventConnect.Fire();

            // The parent state machine's 'CurrentState' is the topmost current state
            Assert.AreEqual(connectingSuperState, parentStateMachine.CurrentState);
            // While its 'CurrentChildState' property indicates the currently-active child state machine's state
            Assert.AreEqual(connectingInitialise, parentStateMachine.CurrentChildState);

            // The child state machine knows that it is active
            Assert.True(connectingStateMachine.IsActive);
            // And knows what its current state is
            Assert.AreEqual(connectingInitialise, connectingStateMachine.CurrentState);


            // When this event is fired, the currently active child state machine (which is connectingStateMachine)
            // is given the chance to handle the event. Since there's a valid transition (from connectingInitialise
            // to connecting), this transition takes place
            eventConnectionInitialised.Fire();

            // As before, this indicates the currently-active child state
            Assert.AreEqual(connecting, parentStateMachine.CurrentChildState);


            // Again, this is handled by the child state machine
            eventConnected.Fire();
            Assert.AreEqual(handshaking, parentStateMachine.CurrentChildState);


            // This is passed to the child state machine, but that does not know how to handle it, so it bubbles
            // up to the parent state machine. This causes a transition from connectingSuperState to
            // connectedSuperState. This causes a number of things, in order:
            // 1. Handshaking's Exit Handler is called. From=Handshaking, To=ConnectedSuperState
            // 2. ConnectingSuperState's Exit Handler is called. From=ConnectingSuperState, To=ConnectedSuperState
            // 3. The transition handler for the transition between ConnectingSuperState and ConnectedSuperState
            //    is called
            // 4. ConnectedSuperState's Entry Handler is called. From=ConnectingSuperState, To=ConnectedSuperState
            // 5. Authorising's Entry Handler is called. From=ConnectingSuperState, To=Authorising
            // 6. The Transition event on the parentStateMachine is raised
            eventHandshakingCompleted.Fire();

            Assert.True(authorising.IsCurrent);

            // This is another transition solely inside a child state machine, from authorising to connected
            eventAuthorisingCompleted.Fire();

            // This is another event which is first sent to the child state machine, then bubbled up to its parent
            // state machine. It causes the following things to occur:
            // 1. Connected's Exit Handler is called. From=Connected, To=Disconnected
            // 2. ConnectedSuperState's Exit Handler is called. From=ConnectedSuperState, To=Disconnected
            // 3. The transition handler from ConnectedSuperState to Disconnected is called.
            //    From=ConnectedSuperState, To=Disconnected
            // 4. Disconnected's Entry Handler is called. From=ConnectedSuperState, To=Disconnected
            // 5. The Transition event on the parentStateMachine is raised
            eventDisconnected.Fire();

            Assert.AreEqual(disconnected, parentStateMachine.CurrentState);

            // Here we show that firing 'disconnected' while in 'connecting' transitions to 'disconnected'.
            // I won't go into as much detail as the previous example.
            eventConnect.Fire();
            eventConnectionInitialised.Fire();
            eventConnected.Fire();
            // This will be handled by the child state machine
            eventDisconnected.Fire();
            Assert.AreEqual(connecting, parentStateMachine.CurrentChildState);
        }

        [Description("Serialization and Deserialization")]
        public static void SerializationAndDeserialization()
        {
            var stateMachine = new StateMachine();
            var stateA = stateMachine.CreateInitialState("StateA");
            var stateB = stateMachine.CreateState("StateB");

            var evt = new Event();
            stateA.TransitionOn(evt).To(stateB);

            // Move us out of the default state
            evt.Fire();
            Assert.AreEqual(stateB, stateMachine.CurrentState);

            string serialized = stateMachine.Serialize();

            // Reset the state machine
            stateMachine.Reset();
            Assert.AreEqual(stateA, stateMachine.CurrentState);

            // Deserialize into it
            stateMachine.Deserialize(serialized);
            Assert.AreEqual(stateB, stateMachine.CurrentState);
        }

        [Description("Custom State Base Subclasses")]
        public static void CustomStateBaseSubclasses()
        {
            // Create a state machine where all states are CustomState (or subclasses)

            var stateMachine = new StateMachine<CustomBaseState>();
            CustomBaseState intiial = stateMachine.CreateInitialState("Initial");

            // Properties on stateMachine refer to CustomStates
            CustomBaseState currentState = stateMachine.CurrentState;
        }

        [Description("Custom Specific State Subclasses")]
        public static void CustomSpecificStateSubclasses()
        {
            var stateMachine = new StateMachine();
            CustomSpecificState normalInitial = stateMachine.CreateInitialState<CustomSpecificState>("Initial");
        }
    }
}
