using System;

namespace StateMechanic
{
    /// <summary>
    /// The result of calling <see cref="StateBase{TState}.TransitionOn(Event)"/> or <see cref="StateBase{TState}.TransitionOn{TEventData}(Event{TEventData})"/>, represents a builder creating a transition
    /// </summary>
    /// <typeparam name="TState">Type of state this transition will be from</typeparam>
    /// <typeparam name="TEventData">Type of event data associted with this transition</typeparam>
    public interface ITransitionBuilder<TState, TEventData> where TState : StateBase<TState>, new()
    {
        /// <summary>
        /// Set the state this transition will transition to
        /// </summary>
        /// <param name="state">State this transition will transition to</param>
        /// <returns>The created transition, to which handlers can be added</returns>
        Transition<TState, TEventData> To(TState state);

        /// <summary>
        /// Create a dynamic transition, where the state to transition to is determined at runtime by a user-provided callback
        /// </summary>
        /// <param name="stateSelector">Callback which will determine which state is transitioned to</param>
        /// <returns>The created transition, to which handlers can be added</returns>
        DynamicTransition<TState, TEventData> ToDynamic(Func<DynamicSelectorInfo<TState, TEventData>, TState> stateSelector);
    }
}
