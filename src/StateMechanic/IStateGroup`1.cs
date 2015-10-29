namespace StateMechanic
{
    internal interface IStateGroup<TState> : IStateGroup
    {
        void FireEntryHandler(StateHandlerInfo<TState> info);
        void FireExitHandler(StateHandlerInfo<TState> info);
    }
}
