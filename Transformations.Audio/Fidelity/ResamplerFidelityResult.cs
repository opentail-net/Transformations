namespace Transformations.Audio.Fidelity;

/// <summary>Result for one algorithm under one fidelity scenario.</summary>
public sealed record ResamplerFidelityResult(
    string AlgorithmId,
    string Algorithm,
    string AlgorithmVersion,
    bool Experimental,
    string Scenario,
    string SignalProfile,
    string EvaluationMode,
    bool Gated,
    int InputRate,
    int OutputRate,
    int Channels,
    int InputFrames,
    int OutputFrames,
    int ExpectedOutputFrames,
    int RoundTripFrames,
    double MeanSquaredError,
    double SignalToNoiseRatioDb,
    double PeakSignalToNoiseRatioDb,
    long ElapsedMilliseconds,
    bool Passed,
    bool MeetsThresholds,
    string? FailureReason);

/// <summary>Aggregate fidelity report for a deterministic suite run.</summary>
public sealed record ResamplerFidelityReport(
    DateTimeOffset StartedAt,
    ResamplerFidelityOptions Options,
    IReadOnlyList<ResamplerFidelityResult> Results)
{
    /// <summary>True when every measured algorithm/scenario pair met the configured thresholds.</summary>
    public bool Passed => Results.Where(r => r.Gated).All(r => r.Passed);
}
