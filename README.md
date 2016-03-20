StateMechanic
=============

StateMechinic is a hierarchical state machine library.
It supports data attached to states, data attached to events, entry/exit/transition handlers, child state machines, and more.


Installation
------------

StateMechinic has not yet been released. Check back soon!


Quick Start
-----------

Here's a quick introduction to the basics.
We'll create a state machine to represent a drinks dispenser.

```csharp
// First off, create a state machine. It's recommended to give it a name
// (which helps debugging), but you don't have to.
var stateMachine = new StateMachine("Sode Machine");

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

// Fire events directly. This will throw an exception if there is no transition
// from the current state.
moneyReceived.Fire();

// Alternatively, you can 

```

You'll notice that states and events are instances, rather than being enums (as in other state machine libraries).


State Entry/Exit Handlers
-------------------------

Just creating a state machine isn't very interesting on its own: we usually want to execute some code when particular things happen.
State entry handlers are executed whenever the state is entered, and likewise an exit handler is always executed when the state is exited.
Creating an entry/exit handler pair is a very powerful tool: you can make sure that the exit handler always undoes whatever the entry handler did.

Entry and exit handlers are passed an object containing various bits of useful information: the states being transitioned from and to, and the event which triggered that transition.

```csharp
var someState = stateMachine.CreateState("Some State")
	.WithEntry(info => Console.WriteLine($"Entry from {info.From} to {info.To} on {info.Event}"))
	.WithExit(info => Console.WriteLine($"Exit from {info.From} to {info.To} on {info.Event}"));

// You can also set the EntryHandler and ExitHandler properties directly
someState.EntryHandler = info => Console.WriteLine($"Entry from {info.From} to {info.To} on {info.Event}");
someState.ExitHandler = info => Console.WriteLine($"Exit from {info.From} to {info.To} on {info.Event}");
```


Transition Handlers
-------------------

You can also register a handler on a transition: whenever that transition occurs, the handler will be called.
As with state entry/exit handlers, transition handlers receive an object containing useful information about the transition.

```csharp
someState.TransitionOn(someEvent).To(someOtherState)
	.WithHandler(info => Console.WriteLine($"Transition from {info.From} to {info.To} on {info.Event}"));

// You can also set the Handler property directly
var transition = someState.TransitionOn(someEvent).To(someOtherState);
transition.Handler = info => Console.WriteLine($"Transition from {info.From} to {info.To} on {info.Event}");
```


Inner Self Transitions
----------------------

A state is allowed to transition to itself.
This is a useful way of running some code when an event is fired, and the state machine is in a particular state.
By default, both the exit and entry handlers will also be invoked in this case.
If you don't want this, then create an inner self transition instead.

```csharp
var state = stateMachine.CreateState("State")
	.WithEntry(i => Console.WriteLine("Entry"))
	.WithExit(i => Console.WriteLine("Exit"));

state.TransitionOn(event1).To(state).WithHandler(i => Console.WriteLine("Handler"));

state.InnerSelfTransitionOn(event2).WithHandler(i => Console.WriteLine("Handler"));

event1.Fire();
// Prints: Exit, Handler, Entry

// event2.Fire()
// Prints: Handler
```


Event Data
----------

You can associate data with particular events. When you fire the event, you also provide the data.
This data is then available to transition handlers and transition guards (see below).

```csharp
// This is an event which takes a string argument (but you can use any data type)
var eventWithData = new Event<string>();

state.TransitionOn(eventWithData).To(anotherState)
	.WithHandler(info => Console.WriteLine($"Data: {info.EventData}"));

// Provide the data when you fire the event
eventWithData.Fire("Some Data");

// Prints: "Data: Some Data"
```


Transition Guards
-----------------

Sometimes life isn't as simple as always transitioning from state A to state B on event E.
Sometimes when event E is fired you might transition from `A -> B` in some cases, but from `A -> C` in others.

One way of achieving this is with transition guards.
A transition guard is a delegate which is controls whether a particular transition can take place.
If it can't, then any other transitions from the current state on that event are tried (transitions are tried in the order in which they were added).

```csharp

bool allowTransitionToStateB = false;

stateA.TranstionOn(eventE).To(stateB).WithGuard(info => allowTransitionToStateB);
stateA.TranstionOn(eventE).To(stateC);

eventE.Fire();
Assert.AreEqual(stateB, stateMachine.CurrentState);

// Alternatively...
allowTransitionToStateB = false;
eventE.Fire();
Assert.AreEqual(stateC, stateMachine.CurrentState);
```

The guard receives an object with useful information about the transition.


Dynamic Transitions
-------------------

Dynamic transitions are the other way of allowing you to specify what state to transition to at runtime.


TODO
---- 

 - Handler order
 - Dynamic transitions
 - Events from within handlers
 - Faulting
 - Stuff on StateMachine
 - Custom states
 - Printing