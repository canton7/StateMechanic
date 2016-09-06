using System;

namespace StateMechanic
{
    /// <summary>
    /// PCLs don't give us Trace, so create our own
    /// </summary>
    internal static class Trace
    {
        [ExcludeFromCoverage]
        public static void Assert(bool condition, string message = null)
        {
            if (!condition)
            {
                var msg = (message == null) ? $"Assertion failed: {message}" : "Assertion Failed";
                throw new AssertionFailedException(msg);
            }
        }
    }

    internal class AssertionFailedException : Exception
    {
        public AssertionFailedException(string message) : base(message) { }
    }
}
