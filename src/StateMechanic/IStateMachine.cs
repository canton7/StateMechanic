using System;
using System.Collections.Generic;

namespace StateMechanic
{
    /// <summary>
    /// A state machine, which may exist as a child state machine, with or without per-state data
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
        IState CurrentStateRecursive { get; }

        /// <summary>
        /// Gets the initial state of this state machine
        /// </summary>
        IState InitialState { get; }

        /// <summary>
        /// Gets a list of all states which are part of this state machine
        /// </summary>
        IReadOnlyList<IState> States { get; }

        /// <summary>
        /// Determines whether this state machine is a child of another state machine
        /// </summary>
        /// <param name="parentStateMachine">State machine which may be a parent of this state machine</param>
        /// <returns>True if this state machine is a child of the given state machine</returns>
        bool IsChildOf(IStateMachine parentStateMachine);

        /// <summary>
        /// Determines whether this state machine is in the given state, or the current state's child state machine is in the given state, recursively
        /// </summary>
        /// <param name="state">The state to test</param>
        /// <returns>True if this state machine is in the given state, or the current state's child state machine is in the given state, recursively</returns>
        bool IsInStateRecursive(IState state);
    }

    internal interface IStateMachine<TState> : IStateMachine, IEventDelegate where TState : class, IState<TState>
    {
        new TState CurrentStateRecursive { get; }
        new TState InitialState { get; }
        new TState CurrentState { get; }
        bool RequestEventFire(IEvent sourceEvent, Func<IState, bool> invoker, EventFireMethod eventFireMethod);
        void SetCurrentState(TState state);
    }
}
