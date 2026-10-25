using System;
using System.Collections.Generic;

namespace Smoke.Text.Support;

/// <summary>Outcome of one check. Failure details name the expected and actual values.</summary>
public sealed record CheckResult(string Name, bool Passed, string Detail, long ElapsedMs);

/// <summary>One named check. Run throws when the expectation does not hold.</summary>
public sealed record SmokeCheck(string Name, Action Run);

public sealed class SmokeFailure : Exception
{
    public SmokeFailure(string message)
        : base(message)
    {
    }
}

public static class Expect
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
}
