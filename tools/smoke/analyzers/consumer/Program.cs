using System;
using Acme;

namespace Smoke.Analyzers.Consumer
{
    public enum Colour
    {
        Red,
        Blue,
    }

    internal static class Program
    {
        // Exactly one deprecated call (expected: one TX0001 warning) and one current call (expected: none).
        private static int Main()
        {
            string deprecated = Colour.Red.GetEnumDescription3();
            string current = Colour.Blue.GetDescription();

            Console.WriteLine($"{deprecated} {current}");
            return 0;
        }
    }
}
