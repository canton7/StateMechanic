using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace StateMechanic
{
    internal class StateInner<TState> where TState : class, IState<TState>
    {
        private readonly ITransitionDelegate<TState> transitionDelegate;

        public IStateDelegate<TState> ChildStateMachine { get; set; }

        public string Name { get; }

        private readonly List<ITransition<TState>> transitions = new List<ITransition<TState>>();
        public IReadOnlyList<ITransition<TState>> Transitions { get; }

        private readonly List<IStateGroup<TState>> groups = new List<IStateGroup<TState>>();
        public IReadOnlyList<IStateGroup<TState>> Groups { get; }

        public Action<StateHandlerInfo<TState>> EntryHandler { get; set; }
        public Action<StateHandlerInfo<TState>> ExitHandler { get; set; }

        internal StateInner(string name, ITransitionDelegate<TState> transitionDelegate)
        {
            this.Name = name;
            this.transitionDelegate = transitionDelegate;
            this.Transitions = new ReadOnlyCollection<ITransition<TState>>(this.transitions);
            this.Groups = new ReadOnlyCollection<IStateGroup<TState>>(this.groups);
        }

        public ITransitionBuilder<TState> TransitionOn(TState fromState, Event @event)
        {
            return new TransitionBuilder<TState>(fromState, @event, this.transitionDelegate);
        }

        public ITransitionBuilder<TState, TEventData> TransitionOn<TEventData>(TState fromState, Event<TEventData> @event)
        {
            return new TransitionBuilder<TState, TEventData>(fromState, @event, this.transitionDelegate);
        }

        public Transition<TState> InnerSelfTransitionOn(TState fromAndToState, Event @event)
        {
            var transition = Transition.CreateInner<TState>(fromAndToState, @event, this.transitionDelegate);
            @event.AddTransition(fromAndToState, transition);
            this.AddTransition(transition);
            return transition;
        }

        public Transition<TState, TEventData> InnerSelfTransitionOn<TEventData>(TState fromAndToState, Event<TEventData> @event)
        {
            var transition = Transition.CreateInner<TState, TEventData>(fromAndToState, @event, this.transitionDelegate);
            @event.AddTransition(fromAndToState, transition);
            this.AddTransition(transition);
            return transition;
        }

        public void AddTransition(ITransition<TState> transition)
        {
            this.transitions.Add(transition);
        }

        public void FireEntryHandler(StateHandlerInfo<TState> info)
        {
            this.EntryHandler?.Invoke(info);
        }

        public void FireExitHandler(StateHandlerInfo<TState> info)
        {
            this.ExitHandler?.Invoke(info);
        }

        public void AddGroup(IStateGroup<TState> group)
        {
            this.groups.Add(group);
        }
    }
}
