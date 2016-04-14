using System.Collections.Generic;

namespace StateMechanic
{
    /// <summary>
    /// A state, which can be transioned from/to, and which can represent the current state of a <see cref="StateMachine{TState}"/>
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// Gets the name assigned to this state
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a value indicating whether this state's parent state machine is in this state
        /// </summary>
        bool IsCurrent { get; }

        /// <summary>
        /// Gets the child state machine of this state, if any
        /// </summary>
        IStateMachine ChildStateMachine { get; }

        /// <summary>
        /// Gets the state machine to which this state belongs
        /// </summary>
        IStateMachine ParentStateMachine { get; }

        /// <summary>
        /// Gets a list of transitions available from the current state
        /// </summary>
        IReadOnlyList<ITransition<IState>> Transitions { get; }

        /// <summary>
        /// Gets the list of groups which this state is a member of
        /// </summary>
        IReadOnlyList<IStateGroup> Groups { get; }
    }
}
