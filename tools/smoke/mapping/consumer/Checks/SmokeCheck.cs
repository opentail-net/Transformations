using System;
using System.Collections.Generic;

namespace Smoke.Mapping.Checks;

// One named expectation. Run throws when the expectation does not hold.
public sealed record SmokeCheck(string Name, Action Run);

public sealed class SmokeFailure : Exception
{
    public SmokeFailure(string message)
        : base(message)
    {
    }
}

internal static class Expect
{
    public static void Equal<T>(T expected, T actual, string label)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new SmokeFailure($"{label}: expected <{expected}>, was <{actual}>");
        }
    }

    public static void True(bool condition, string label)
    {
        if (!condition)
        {
            throw new SmokeFailure(label);
        }
    }

    public static TException Throws<TException>(Action action, string label)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException expected)
        {
            return expected;
        }

        throw new SmokeFailure($"{label}: expected {typeof(TException).Name}, nothing was thrown");
    }
}
