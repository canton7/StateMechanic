namespace StateMechanic
{
    internal interface ITransition
    {
        bool WillAlwaysOccur { get; }
    }
}
