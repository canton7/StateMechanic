Changelog
=========

1.0.3
-----

 - When entering a StateGroup, call the StateGroup's entry handler before calling the State's entry handler (#3)
 - Pass EventData to State overridable methods
 - Generally reduce allocations

1.0.2
-----

 - Make EventData available (as an `object`) to state entry and exit handlers, transitions, and others
 - Raise the `Transition` event when a transition begins. Add a `TransitionFinished` event to indicate when the transition finishes
 - Pass EventFireInfo to many more events and handlers
 - Add an option to render Dot state machine graphs vertically
 - Add support for multiple links from and to the same states to DGML state machine graphs
 - Add Ignored events
 - Add an exception if the user configures a transition that can never occur

1.0.1
-----

 - Add the ability to render state machines to DGML