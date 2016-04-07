using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samples
{
    /// <summary>
    /// Helpers for the examples
    /// </summary>
    public static class Assert
    {
        public static void AreEqual<T>(T expected, T actual)
        {
            if (EqualityComparer<T>.Default.Equals(expected, actual))
            {
                Console.WriteLine($"Assert.Equal({actual})");
            }
            else
            {
                throw new AssertionException($"Expected {expected}, got {actual}");
            }
        }

        public static void True(bool value)
        {
            if (value)
            {
                Console.WriteLine("Assert.True(true)");
            }
            else
            {
                throw new AssertionException($"Expected true, got false");
            }
        }

        public static void False(bool value)
        {
            if (!value)
            {
                Console.WriteLine("Assert.False(false)");
            }
            else
            {
                throw new AssertionException($"Expected false, got true");
            }
        }
    }

    public class AssertionException : Exception
    {
        public AssertionException(string message)
            : base(message)
        {
        }
    }
}
