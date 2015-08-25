using System;
using System.Collections.Generic;

namespace StateMechanic
{
    /// <summary>
    /// A group, which contains many states, and can have its own entry and exit ahndlers
    /// </summary>
    public class StateGroup<TStateData> : IStateGroup<State<TStateData>>
    {
        private readonly StateGroupInner<State<TStateData>> innerStateGroup;

        /// <summary>
        /// Initialises a new instance of the <see cref="StateGroup{TStateData}"/> class
        /// </summary>
        /// <param name="name">Name of this state group</param>
        public StateGroup(string name)
        {
            this.innerStateGroup = new StateGroupInner<State<TStateData>>(name);
        }

        /// <summary>
        /// Gets the name given to this group
        /// </summary>
        public string Name
        {
            get { return this.innerStateGroup.Name; }
        }

        /// <summary>
        /// Gets a value indicating whether any member of this group is its state machine's current state
        /// </summary>
        public bool IsCurrent
        {
            get { return this.innerStateGroup.IsCurrent; }
        }

        /// <summary>
        /// Gets a list of all states which are a member of this group
        /// </summary>
        public IReadOnlyList<State<TStateData>> States
        {
            get { return this.innerStateGroup.States; }
        }

        IReadOnlyList<IState> IStateGroup.States
        {
            get { return this.innerStateGroup.States; }
        }

        /// <summary>
        /// Gets or sets the method called when the StateMachine enters this state
        /// </summary>
        public Action<StateHandlerInfo<State<TStateData>>> EntryHandler
        {
            get { return this.innerStateGroup.EntryHandler; }
            set { this.innerStateGroup.EntryHandler = value; }
        }

        /// <summary>
        /// Gets or sets the method called when the StateMachine exits this state
        /// </summary>
        public Action<StateHandlerInfo<State<TStateData>>> ExitHandler
        {
            get { return this.innerStateGroup.ExitHandler; }
            set { this.innerStateGroup.ExitHandler = value; }
        }

        /// <summary>
        /// Set the method called when the StateMachine enters this state
        /// </summary>
        /// <param name="entryHandler">Method called when the StateMachine enters this state</param>
        /// <returns>This State, used for method chaining</returns>
        public StateGroup<TStateData> WithEntry(Action<StateHandlerInfo<State<TStateData>>> entryHandler)
        {
            this.EntryHandler = entryHandler;
            return this;
        }

        /// <summary>
        /// Set the method called when the StateMachine exits this state
        /// </summary>
        /// <param name="exitHandler">Method called when the StateMachine exits this state</param>
        /// <returns>This State, used for method chaining</returns>
        public StateGroup<TStateData> WithExit(Action<StateHandlerInfo<State<TStateData>>> exitHandler)
        {
            this.ExitHandler = exitHandler;
            return this;
        }

        void IStateGroup<State<TStateData>>.FireEntryHandler(StateHandlerInfo<State<TStateData>> info)
        {
            this.innerStateGroup.FireEntryHandler(info);
        }

        void IStateGroup<State<TStateData>>.FireExitHandler(StateHandlerInfo<State<TStateData>> info)
        {
            this.innerStateGroup.FireExitHandler(info);
        }

        internal void AddState(State<TStateData> state)
        {
            this.innerStateGroup.AddState(state);
        }
    }
}
