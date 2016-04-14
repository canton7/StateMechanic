namespace StateMechanic
{
    internal interface IStateDelegate<TState>
    {
        TState InitialState { get; }
        TState CurrentState { get; }
    }
}
