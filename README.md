StateMechanic
=============

StateMechinic is a hierarchical state machine library.
It supports data attached to states, data attached to events, entry/exit/transition handlers, child state machines, and more.


Installation
------------

StateMechinic has not yet been released. Check back soon!


Quick Start
-----------

First, create your state machine.
It's a good idea to give it a name: this isn't used internally, but is very useful for debugging.

```csharp
var stateMachine = new StateMachine("Telephone Call");
```

Next, create your states and events.
As before, it's useful for debugging purposes to give them names.

```csharp
var stateOffHook = stateMachine.CreateState("Off Hook");
```

You'll notice that this approach differs from other state machine libraries: states and events are instances rather than enum values.
This has a few advantages, as we'll see later.
