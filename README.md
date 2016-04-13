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
var state = stateMachine.CreateInitialState("State")
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

stateA.TransitionOn(eventE).To(stateB).WithGuard(info => allowTransitionToStateB);
stateA.TransitionOn(eventE).To(stateC);

eventE.Fire();
Assert.AreEqual(stateC, stateMachine.CurrentState);

// Alternatively...
allowTransitionToStateB = true;
eventE.Fire();
Assert.AreEqual(stateB, stateMachine.CurrentState);
```

The guard receives an object with useful information about the transition.


Dynamic Transitions
-------------------

Dynamic transitions are the other way of allowing you to specify what state to transition to at runtime.
Instead of providing a state to transition to, you instead provide a delegate that, when called, gives the state to transition to.

These are slightly less clear than transition guards (especially when printing the state machine, see later).

```csharp
State stateToTransitionTo = stateB;

stateA.TransitionOn(eventE).ToDynamic(info => stateToTransitionTo);

eventE.Fire();
Assert.AreEqual(stateB, stateMachine.CurrentState);

// Alternatively...
stateToTransitionTo = stateC;
eventE.Fire();
Assert.AreEqual(stateC, stateMachine.CurrentState);
```

You can return `null` from the delegate given to `ToDynamic`, in which case the next eligible transition is attempted (i.e. the same as if the transition had a guard, which returned false).


Recursive Transitions
---------------------

You are allowed to fire events from within state entry and exit handlers, and from transition handlers.
The event is queued until the current transition is complete, at which point it is fired.

Because events are queued, the behaviour around `Event.Fire()` and `Event.TryFire()` changes a bit.

`Event.Fire()` will never throw an exception directly, but when that event is dequeued it will throw an exception if no transition from the current state (at that point) on that event exists, and that will bubble back to whatever method (`Event.Fire()` **or** `Event.TryFire()`) was used to kick off the outermost transition.

`Event.TryFire()` will always return true.
When that event is dequeued, if no transition from the current state (at that point) on that event exists, then any queued events are not affected.
You will have no way of knowing that firing the event failed (other than the `TransitionNotFound` event on the StateMachine).


Exceptions and Faulting
-----------------------

State entry/exit handlers and transition handlers may not throw exceptions.
(If exceptions were allowed, then these handlers would not run to completion, and it would be impossible to guarentee the integrity of the state machine).

If one of these handlers does throw an exception, then a `TransitionFailedException` will be thrown, and the state machine will enter a faulted state.
The `Faulted` event on the StateMachine will also be raised, and its `Fault` property will contain information about the fault.

When a state machine has faulted, no more transitions may occur, and the current state may not be queried.
The state machine will raise the `Faulted` event, then `IsFaulted` property returns true, and the `Fault` property will give information on the fault.
The only way to recovert is to call `Reset()` on the state machine, which will reset it to its initial state and clear the fault.

Transition guards and dynamic transition state selectors may throw exceptions: these exceptions are propagated back to the `Event.Fire()` or `Event.TryFire()` call which initiated the transition, and the transition will not occur.


State Groups
------------

State groups provide a lightweight alternative to full-blown hierarchical state machines (documented below).
A state group can contain one or more states, and a state can be part of one of more state groups.

State groups have an `IsCurrent` property, which indicates whether any of their states are current.
This can be useful if you have, for example, multiple states which together indicate that a device is connecting to another device.
You can add all of these 'connecting' states to a state group, and use the state groups 'IsCurrent' property to determine whether the state machine is in any of the states related to connecting.

State groups also have their own Exit and Entry handlers.
The Entry handler is invoked if the state machine transitions from a state which is not part of the state group, to a state which is.
Likewise the Exit handler is invoked if the state machine transitions from a state which is part of the state group to a state which is not.

```csharp
var stateA = stateMachine.CreateState("State A");
var stateB = stateMachine.CreateState("State B");

// You can create state groups, and add states to them
var statesAAndB = new StateGroup("States A and B")
    .WithEntry(info => Console.WriteLine($"Entering group from {info.From} to {info.To} on {info.Event}"))
    .WithExit(info => Console.WriteLine($"Exiting group from {info.From} to {info.To} on {info.Event}"));

statesAAndB.AddStates(stateA, stateB);

// You can also add states to groups
stateA.AddToGroup(statesAAndB);
```


Child State Machines
--------------------

StateMechanic supports fully hierarchical state machines.
The model is that any state may have a single child state machine, which are fully-fledges state machines in their own right.

Child state machines start by not existing in any state (`CurrentState` is `null`).
When a sparent tate machine transitions to a state which has a child state machine, that child state machine will also transition to its initial state.
This child state machine is now active.

When a parent state machine transitions away from a state which has a child state machine, that child state machine will first transition away from its current state.
The parent state machine will then transition to the next state.

The order in which handlers are called is detailed in the code example below.

When an event is fired, any active child state machines are given the opportunity to handle the event: if a transition exists from the child machine's current state to another state, then that transition will be invoked.
If no transitions are found, then the parent of that child attempts to handle the event, and so on.

States belonging to a state machine may not transition to states belonging to a different (parent or child) state machine.
The only way that a child state machine can be exited is if a transition occurs on its parent which changes the current state away from the state which owns that child state machine.

A state machine's `CurrentState` will never refer to a state belonging to a child state machine.
The `CurrentChildState` property will, however, refer to the child-most active state.

All of this is quite confusing!
Here's an example to hopefully clear the smoke.

```csharp
var parentStateMachine = new StateMachine("Parent");

var disconnected = parentStateMachine.CreateInitialState("Disconnected");
var connectingSuperState = parentStateMachine.CreateState("ConnectingSuperState");
var connectedSuperState = parentStateMachine.CreateState("ConnectedSuperState");

// 'Connecting' child state machine
var connectingStateMachine = connectingSuperState.CreateChildStateMachine("ConnectingSM");
var connectingInitialise = connectingStateMachine.CreateInitialState("ConnectingInitialisation");
var connecting = connectingStateMachine.CreateState("Connecting");
var handshaking = connectingStateMachine.CreateState("Handshaking");

// 'Connected' child state machine
var connectedStateMachine = connectedSuperState.CreateChildStateMachine("ConnectedSM");
var authorising = connectedStateMachine.CreateInitialState("Authorising");
var connected = connectedStateMachine.CreateState("Connected");

// Events
var eventConnect = new Event("Connect");
var eventConnectionInitialised = new Event("Connecting Initialised");
var eventConnected = new Event("Connected");
var eventHandshakingCompleted = new Event("Handshaking Completed");
var eventAuthorisingCompleted = new Event("Authorising Completed");
var eventDisconnected = new Event("Disconnected");

disconnected.TransitionOn(eventConnect).To(connectingSuperState);
connectingInitialise.TransitionOn(eventConnectionInitialised).To(connecting);
connecting.TransitionOn(eventConnected).To(handshaking);
handshaking.TransitionOn(eventDisconnected).To(connecting);
connectingSuperState.TransitionOn(eventHandshakingCompleted).To(connectedSuperState);
authorising.TransitionOn(eventAuthorisingCompleted).To(connected);
connectingSuperState.TransitionOn(eventDisconnected).To(disconnected);
connectedSuperState.TransitionOn(eventDisconnected).To(disconnected);


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
```


Forced Transitions
------------------

Forced transitions are a mechism for forcibly transitioning between two states, regardless of whether there's a defined transition between them.
They're strongly discouraged - the whole advantage of a state machine is that the possible transitions between states is very clear and explicit, and forced transitions violate this - but sometimes there's no alternative.

When you force a transition, you specify the state to transition to, and an event.
The event is passed to state exit and entry handlers (you're pretending that that event caused the transition), but that is all.

You may force a transition to any state, even ones that belong to child or parent state machines, and the appropriate exit and entry handlers will be called.



Thread Safety
-------------

StateMechanic is not thread safe by default.

It does however provide a mechanism for providing thread safety.
You can assign an `IStateMachineSynchronizer` to `StateMachine.Synchronizer`.
Methods on the synchronizer will be invoked when events are fired, and the synchronizer ensures that these are given to the rest of the StateMachine in a thread-safe way (i.e. one at a time).
See the remarks on that interface for more information.

A simple locking implementation, `LockingStateMachineSynchronizer`, exists.
This will cause calls to `Event.Fire()`, `Event.TryFire()`, `StateMachine.Reset()`, and `StateMachine.ForceTransition()` to block until any preceding transitions have finished.

Note that it is not recommended to perform long-running or blocking operations in any handlers: this leaves the state machine in a mid-way point between two states, which is inconsistent.
Instead, use a state to represent the fact that the long-running operation is ongoing, but don't block any handlers to perform this operation (e.g. 'Connecting'), and transition away from it when the operation completes.


Serialization
-------------

It can be useful to serialize a state machine, for example to a database, and restore it to a later date.

StateMechanic supports this, with the following restrictions:

1. The state machine which is deserialized into must be identical to that which was serialized. If it is not, deserialization may produce the wrong result, or it may fail completely.
2. The serialization format may change between minor StateMechanic versions. This will cause deserialization to fail in a controlled manner.

For example:

```csharp
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
```

If you wish, you can customize how serialization occurs by implementing `IStateMachineSerializer<TState>`, then setting the `StateMachine.Serializer` property.


Printing State Machines
-----------------------

It's very useful to be able to visualise your state machines: it lets you easily spot and debug mistakes you made when setting it up, and they make fantastic inputs to your documentation ("this is our state machine, and I know that it's correct").

StateMechanic can take a state machine and create a [Graphviz DOT](http://www.graphviz.org/content/dot-language) file, which can be rendered to a utility using the `dot` executable included in the Graphviz package (or [loads](http://dreampuf.github.io/GraphvizOnline/) [of](http://sandbox.kidstrythisathome.com/erdos/) [online](http://stamm-wilbrandt.de/GraphvizFiddle) [utilities](http://www.webgraphviz.com)).

To render a state machine, call `StateMachine.FormatDot()`.


Custom State Subclasses
-----------------------

StateMechanic's default mode is to define entry/exit handlers, guard, and dynamic transition handlers using delegates, provided using the fluent syntax used elsewhere in this document.
However, you can define your own custom state classes if you wish.
This allows you to encapsulate more complex logic more easily.

Subclasses cannot be used to specify transition handlers.

The are two ways specify custom state classes: a custom base state for the entire state machine, and custom state classes for individual states.

### Custom Base State

By default, the `State` class is used everywhere in StateMechanic, when looking at e.g. `StateMachine.CurrentState`, in information given to various handlers, etc.
However, you can customize this.

First, derive from `StateBase<T>`.
The `T` here has to be a reference to your class type: this is an unfortunate artefact of C#'s type system, and is checked at runtime.

```csharp
class CustomBaseState : StateBase<CustomBaseState>
{
    // There are various methods you can override
}

var stateMachine = new StateMachine<CustomBaseState>();
CustomBaseState initial = stateMachine.CreateInitialState("Initial");
// Properties on stateMachine refer to CustomStates
CustomBaseState currentState = stateMachine.CurrentState;
```




TODO
---- 

 - Stuff on StateMachine (??)
 - Custom states