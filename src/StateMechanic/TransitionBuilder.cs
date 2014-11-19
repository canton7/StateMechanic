using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public interface ITransitionBuilder<TState>
    {
        Transition<TState> To(TState state);
    }

    internal class TransitionBuilder<TState> : ITransitionBuilder<TState>
    {
        private readonly TState fromState;
        private readonly Event evt;
        private readonly Action<Event, Transition<TState>> adder;

        public TransitionBuilder(TState fromState, Event evt, Action<Event, Transition<TState>> adder)
        {
            this.fromState = fromState;
            this.evt = evt;
            this.adder = adder;
        }

        Transition<TState> ITransitionBuilder<TState>.To(TState state)
        {
            var transition = new Transition<TState>(this.fromState, state, this.evt);
            this.adder(this.evt, transition);
            return transition;
        }
    }

    public interface ITransitionBuilder<TState, TEventData>
    {
        Transition<TState, TEventData> To(TState state);
    }

    internal class TransitionBuilder<TState, TEventData> : ITransitionBuilder<TState, TEventData>
    {
        private readonly TState fromState;
        private readonly Event<TEventData> evt;
        private readonly Action<Event<TEventData>, Transition<TState, TEventData>> adder;

        public TransitionBuilder(TState fromState, Event<TEventData> evt, Action<Event<TEventData>, Transition<TState, TEventData>> adder)
        {
            this.fromState = fromState;
            this.evt = evt;
            this.adder = adder;
        }

        Transition<TState, TEventData> ITransitionBuilder<TState, TEventData>.To(TState state)
        {
            var transition = new Transition<TState, TEventData>(this.fromState, state, this.evt);
            this.adder(this.evt, transition);
            return transition;
        }
    }
}
