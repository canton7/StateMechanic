using System.Collections.Generic;

namespace StateMechanic
{
    /// <summary>
    /// A state machine, which may exist as a child state machine
    /// </summary>
    public interface IStateMachine
    {
        /// <summary>
        /// Gets the name given to this state machine when it was created
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the state which this state machine is currently in
        /// </summary>
        IState CurrentState { get; }

        /// <summary>
        /// If <see cref="CurrentState"/> has a child state machine, gets that child state machine's current state (recursively), otherwise gets <see cref="CurrentState"/>
        /// </summary>
        IState CurrentChildState { get; }

        /// <summary>
        /// Gets the initial state of this state machine
        /// </summary>
        IState InitialState { get; }

        /// <summary>
        /// Gets a list of all states which are part of this state machine
        /// </summary>
        IReadOnlyList<IState> States { get; }

        /// <summary>
        /// Gets the parent state of this state machine, or null if there is none
        /// </summary>
        IState ParentState { get; }

        /// <summary>
        /// Gets the parent of this state machine, or null if there is none
        /// </summary>
        IStateMachine ParentStateMachine { get; }

        /// <summary>
        /// Gets the top-most state machine in this state machine hierarchy (which may be 'this')
        /// </summary>
        IStateMachine TopmostStateMachine { get; }

        /// <summary>
        /// Gets a value indicating whether the current state machine is active
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Determines whether this state machine is a child of another state machine
        /// </summary>
        /// <param name="parentStateMachine">State machine which may be a parent of this state machine</param>
        /// <returns>True if this state machine is a child of the given state machine</returns>
        bool IsChildOf(IStateMachine parentStateMachine);
    }
}
