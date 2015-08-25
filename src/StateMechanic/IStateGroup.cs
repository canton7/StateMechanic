using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    /// <summary>
    /// A group, which contains many states, and can have its own entry and exit ahndlers
    /// </summary>
    public interface IStateGroup
    {
        /// <summary>
        /// Gets the name given to this group
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a list of all states which are a member of this group
        /// </summary>
        IReadOnlyList<IState> States { get; }
    }

    internal interface IStateGroup<TState> : IStateGroup
    {
        void FireEntryHandler(StateHandlerInfo<TState> info);
        void FireExitHandler(StateHandlerInfo<TState> info);
    }
}
