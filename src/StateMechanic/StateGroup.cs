using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace StateMechanic
{
    /// <summary>
    /// A group, which contains many states, and can have its own entry and exit ahndlers
    /// </summary>
    public class StateGroup : IStateGroup<State>
    {
        private readonly StateGroupInner<State> innerStateGroup;

        /// <summary>
        /// Initialises a new instance of the <see cref="StateGroup"/> class
        /// </summary>
        /// <param name="name">Name of this state group</param>
        public StateGroup(string name)
        {
            this.innerStateGroup = new StateGroupInner<State>(name);
        }

        /// <summary>
        /// Gets the name given to this group
        /// </summary>
        public string Name => this.innerStateGroup.Name;

        /// <summary>
        /// Gets a value indicating whether any member of this group is its state machine's current state
        /// </summary>
        public bool IsCurrent => this.innerStateGroup.IsCurrent;

        /// <summary>
        /// Gets a list of all states which are a member of this group
        /// </summary>
        public IReadOnlyList<State> States => this.innerStateGroup.States;

        IReadOnlyList<IState> IStateGroup.States => this.innerStateGroup.States;

        /// <summary>
        /// Gets or sets the method called when the StateMachine enters this state
        /// </summary>
        public Action<StateHandlerInfo<State>> EntryHandler
        {
            get { return this.innerStateGroup.EntryHandler; }
            set { this.innerStateGroup.EntryHandler = value; }
        }

        /// <summary>
        /// Gets or sets the method called when the StateMachine exits this state
        /// </summary>
        public Action<StateHandlerInfo<State>> ExitHandler
        {
            get { return this.innerStateGroup.ExitHandler; }
            set { this.innerStateGroup.ExitHandler = value; }
        }

        /// <summary>
        /// Set the method called when the StateMachine enters this state
        /// </summary>
        /// <param name="entryHandler">Method called when the StateMachine enters this state</param>
        /// <returns>This State, used for method chaining</returns>
        public StateGroup WithEntry(Action<StateHandlerInfo<State>> entryHandler)
        {
            this.EntryHandler = entryHandler;
            return this;
        }

        /// <summary>
        /// Set the method called when the StateMachine exits this state
        /// </summary>
        /// <param name="exitHandler">Method called when the StateMachine exits this state</param>
        /// <returns>This State, used for method chaining</returns>
        public StateGroup WithExit(Action<StateHandlerInfo<State>> exitHandler)
        {
            this.ExitHandler = exitHandler;
            return this;
        }

        /// <summary>
        /// Add the given state to this group
        /// </summary>
        /// <param name="state">State to add to this group</param>
        public void AddState(State state)
        {
            state.AddToGroup(this);
        }

        /// <summary>
        /// Add the given states to this group
        /// </summary>
        /// <param name="states">States to add to this group</param>
        public void AddStates(params State[] states)
        {
            foreach (var state in States)
            {
                state.AddToGroup(this);
            }
        }

        void IStateGroup<State>.FireEntryHandler(StateHandlerInfo<State> info)
        {
            this.innerStateGroup.FireEntryHandler(info);
        }

        void IStateGroup<State>.FireExitHandler(StateHandlerInfo<State> info)
        {
            this.innerStateGroup.FireExitHandler(info);
        }

        internal void AddStateInternal(State state)
        {
            this.innerStateGroup.AddState(state);
        }
    }
}
