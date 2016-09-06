using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    internal class RuntimeTransition<TState> : ITransitionInfo<TState>, IStateHandlerInfo<TState>
        where TState : StateBase<TState>, new()
    {
        private readonly Transition<TState> transition;

        public Event Event { get; }
        public EventFireMethod EventFireMethod { get; }

        public TState From => this.transition.From;

        public bool IsInnerTransition => this.transition.IsInnerTransition;

        public TState To => this.transition.To;

        IEvent IStateHandlerInfo<TState>.Event => this.Event;

        public RuntimeTransition(Transition<TState> transition, Event @event, EventFireMethod eventFireMethod)
        {
            this.transition = transition;
            this.Event = @event;
            this.EventFireMethod = eventFireMethod;
        }
    }
}
