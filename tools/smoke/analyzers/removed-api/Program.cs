using System;
using Transformations;

namespace Smoke.Analyzers.Removed
{
    public enum Colour
    {
        Red,
    }

    internal static class Program
    {
        // GetEnumDescription3 was removed from the library in 3.0.0.0. This must not compile.
        private static int Main()
        {
            Console.WriteLine(Colour.Red.GetEnumDescription3());
            return 0;
        }
    }
}
