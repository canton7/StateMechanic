using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    /// <summary>
    /// A state, which can be transioned from/to, and which can represent the current state of a <see cref="StateMachine"/>
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// Gets the name assigned to this state
        /// </summary>
        string Name { get; }

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
    }

    internal interface IState<TState> : IState where TState: class, IState<TState>
    {
        new IStateMachine<TState> ParentStateMachine { get; }
        new IStateMachine<TState> ChildStateMachine { get; }
        new IReadOnlyList<ITransition<TState>> Transitions { get; }
        void AddTransition(ITransition<TState> transition);
        void FireEntryHandler(StateHandlerInfo<TState> info);
        void FireExitHandler(StateHandlerInfo<TState> info);
        void Reset();
    }
}
