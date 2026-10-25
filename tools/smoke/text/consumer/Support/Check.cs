using System;
using System.Collections.Generic;

namespace Smoke.Text.Support;

/// <summary>
/// Outcome of one check. Failure details name the expected and actual values. A skipped check is neither a pass nor a
/// failure: it did not run, and <see cref="Detail"/> says why.
/// </summary>
public sealed record CheckResult(string Name, bool Passed, bool Skipped, string Detail, long ElapsedMs);

/// <summary>One named check. Run throws when the expectation does not hold.</summary>
public sealed record SmokeCheck(string Name, Action Run);

public sealed class SmokeFailure : Exception
{
    public SmokeFailure(string message)
        : base(message)
    {
    }
}

/// <summary>Thrown when a check cannot run here, for example a Tesseract check with no trained data configured.</summary>
public sealed class SmokeSkip : Exception
{
    public SmokeSkip(string reason)
        : base(reason)
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
