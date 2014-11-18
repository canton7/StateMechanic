using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    internal delegate void HandlerInvoker<TState>(TransitionHandler<TState> handler, TState from, TState to);
    internal delegate void HandlerInvoker<TState, TEventData>(TransitionHandler<TState, TEventData> handler, TState from, TState to);

    public class Event<TEventData> : IEvent
    {
        public string Name { get; private set; }
        private readonly Action<Action> firer;

        internal Event(string name, Action<Action> firer)
        {
            this.Name = name;
            this.firer = firer;
        }

        public void Fire(TEventData eventData)
        {

        }
    }

    public class Event : IEvent
    {
        private readonly Event<object> innerEvent;

        internal Event(string name, Action<Action> firer)
        {
            this.innerEvent = new Event<object>(name, firer);
        }

        public void Fire()
        {

        }
    }
}
