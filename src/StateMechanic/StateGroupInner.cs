using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace StateMechanic
{
    internal class StateGroupInner<TState> where TState : IState
    {
        private readonly List<TState> states = new List<TState>();
        public IReadOnlyList<TState> States { get; private set; }

        public string Name { get; private set; }

        public StateGroupInner(string name)
        {
            this.Name = name;
            this.States = new ReadOnlyCollection<TState>(this.states);
        }

        public bool IsCurrent
        {
            get { return this.states.Any(x => x.IsCurrent); }
        }

        public Action<StateHandlerInfo<TState>> EntryHandler { get; set; }
        public Action<StateHandlerInfo<TState>> ExitHandler { get; set; }

        public void FireEntryHandler(StateHandlerInfo<TState> info)
        {
            var handler = this.EntryHandler;
            if (handler != null)
                handler(info);
        }

        public void FireExitHandler(StateHandlerInfo<TState> info)
        {
            var handler = this.ExitHandler;
            if (handler != null)
                handler(info);
        }

        public void AddState(TState state)
        {
            this.states.Add(state);
        }
    }
}
