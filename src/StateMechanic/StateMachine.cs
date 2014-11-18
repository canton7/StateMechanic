using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public class StateMachine
    {
        private readonly StateMachine<object> innerStateMachine;

        public StateMachine(string name)
        {
            this.innerStateMachine = new StateMachine<object>(name);
        }

        public State CreateState(string name)
        {
            return new State(name);
        }

        public Event CreateEvent(string name)
        {
            return this.innerStateMachine.CreateEvent(name);
        }

        public Event<TEventData> CreateEvent<TEventData>(string name)
        {
            return this.innerStateMachine.CreateEvent<TEventData>(name);
        }
    }

    public class StateMachine<TStateData>
    {
        public State<TStateData> CurrentState { get; private set; }
        public string Name { get; private set; }

        public StateMachine(string name)
        {
            this.Name = name;
        }

        public State<TStateData> CreateState(string name)
        {
            return new State<TStateData>(name);
        }

        public Event CreateEvent(string name)
        {
            return new Event(name, null);
        }

        public Event<TEventData> CreateEvent<TEventData>(string name)
        {
            return new Event<TEventData>(name, null);
        }

    }
}
