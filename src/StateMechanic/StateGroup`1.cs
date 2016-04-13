using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace StateMechanic
{
    /// <summary>
    /// A group, which contains many states, and can have its own entry and exit handlers
    /// </summary>
    public class StateGroup<TState> : IStateGroup where TState : StateBase<TState>, new()
    {
        private readonly List<TState> states = new List<TState>();   

        /// <summary>
        /// Gets the name given to this group
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a value indicating whether any member of this group is its state machine's current state
        /// </summary>
        public bool IsCurrent => this.states.Any(x => x.IsCurrent);

        /// <summary>
        /// Gets a list of all states which are a member of this group
        /// </summary>
        public IReadOnlyList<TState> States { get; }

        IReadOnlyList<IState> IStateGroup.States => this.States;

        /// <summary>
        /// Gets or sets the method called when the StateMachine enters this state
        /// </summary>
        public Action<StateHandlerInfo<TState>> EntryHandler { get; set; }

        /// <summary>
        /// Gets or sets the method called when the StateMachine exits this state
        /// </summary>
        public Action<StateHandlerInfo<TState>> ExitHandler { get; set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="StateGroup{TState}"/> class
        /// </summary>
        /// <param name="name">Name of this state group</param>
        public StateGroup(string name = null)
        {
            this.States = new ReadOnlyCollection<TState>(this.states);
            this.Name = name;
        }

        /// <summary>
        /// Set the method called when the StateMachine enters this state
        /// </summary>
        /// <param name="entryHandler">Method called when the StateMachine enters this state</param>
        /// <returns>This State, used for method chaining</returns>
        public StateGroup<TState> WithEntry(Action<StateHandlerInfo<TState>> entryHandler)
        {
            this.EntryHandler = entryHandler;
            return this;
        }

        /// <summary>
        /// Set the method called when the StateMachine exits this state
        /// </summary>
        /// <param name="exitHandler">Method called when the StateMachine exits this state</param>
        /// <returns>This State, used for method chaining</returns>
        public StateGroup<TState> WithExit(Action<StateHandlerInfo<TState>> exitHandler)
        {
            this.ExitHandler = exitHandler;
            return this;
        }

        /// <summary>
        /// Add the given state to this group
        /// </summary>
        /// <param name="state">State to add to this group</param>
        public void AddState(TState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            state.AddToGroup(this);
        }

        /// <summary>
        /// Add the given states to this group
        /// </summary>
        /// <param name="states">States to add to this group</param>
        public void AddStates(params TState[] states)
        {
            if (states == null)
                throw new ArgumentNullException(nameof(states));

            foreach (var state in states)
            {
                state.AddToGroup(this);
            }
        }

        /// <summary>
        /// Invoke the entry handler - override for custom behaviour
        /// </summary>
        /// <param name="info">Information associated with this transition</param>
        protected internal virtual void OnEntry(StateHandlerInfo<TState> info)
        {
            this.EntryHandler?.Invoke(info);
        }

        /// <summary>
        /// Invoke the exit handler - override for custom behaviour
        /// </summary>
        /// <param name="info">Information associated with this transition</param>
        protected internal virtual void OnExit(StateHandlerInfo<TState> info)
        {
            this.ExitHandler?.Invoke(info);
        }

        internal void AddStateInternal(TState state)
        {
            if (!this.states.Contains(state))
                this.states.Add(state);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"<StateGroup Name={this.Name ?? "(unnamed)"} States=[{String.Join(", ", this.States.Select(x => x.ToString()))}>";
        }
    }
}
