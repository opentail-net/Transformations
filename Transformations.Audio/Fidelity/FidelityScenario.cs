namespace Transformations.Audio.Fidelity;

/// <summary>Deterministic signal shape used by a fidelity scenario.</summary>
public enum FidelitySignalProfile
{
    /// <summary>Layered tones below the destination Nyquist frequency.</summary>
    HarmonicBlend,

    /// <summary>Swept tone that exercises interpolation across a broad frequency range.</summary>
    FrequencySweep,

    /// <summary>Sharp transients that expose ringing and timing errors.</summary>
    TransientPulse,

    /// <summary>Deterministic broadband signal for stress-testing accumulated error.</summary>
    DeterministicNoise,

    /// <summary>Near-Nyquist source content that should be suppressed during downsampling.</summary>
    AliasStress,

    /// <summary>Speech-like vowels, fricatives, plosives, and pauses.</summary>
    SpeechFixture,

    /// <summary>Music-like bass, chords, lead partials, stereo movement, and amplitude shaping.</summary>
    MusicFixture,

    /// <summary>Percussion-like kicks, sharp attacks, and ringing decays.</summary>
    PercussionFixture,

    /// <summary>Outdoor/room-like low-frequency rumble, tonal hum, broadband hiss, and sparse events.</summary>
    FieldRecordingFixture,

    /// <summary>High-frequency but mostly in-band content that stresses transition-band behavior.</summary>
    HighFrequencyFixture
}

/// <summary>How a scenario should be measured.</summary>
public enum FidelityEvaluationMode
{
    /// <summary>Resample to the target rate, resample back, and compare with the original signal.</summary>
    RoundTrip,

    /// <summary>Resample once and compare with a generated target-rate reference signal.</summary>
    OneWayReference
}

/// <summary>
/// Describes a deterministic resampling scenario used to evaluate conversion fidelity.
/// </summary>
public sealed record FidelityScenario(
    string Name,
    int InputRate,
    int OutputRate,
    int Channels,
    double DurationSeconds,
    FidelitySignalProfile Profile = FidelitySignalProfile.HarmonicBlend,
    FidelityEvaluationMode EvaluationMode = FidelityEvaluationMode.RoundTrip,
    bool Gated = true)
{
    /// <summary>Default scenarios that cover speech and music-style sample-rate changes.</summary>
    public static IReadOnlyList<FidelityScenario> Defaults { get; } =
    [
        new("Speech 48k to 16k mono", 48_000, 16_000, 1, 1.0),
        new("Music 48k to 44.1k stereo", 48_000, 44_100, 2, 1.0),
        new("Upsample 16k to 48k mono", 16_000, 48_000, 1, 1.0),
        new("Sweep 48k to 32k mono", 48_000, 32_000, 1, 1.0, FidelitySignalProfile.FrequencySweep, Gated: false),
        new("Transient 44.1k to 48k stereo", 44_100, 48_000, 2, 1.0, FidelitySignalProfile.TransientPulse),
        new("Broadband 96k to 44.1k stereo", 96_000, 44_100, 2, 0.5, FidelitySignalProfile.DeterministicNoise, Gated: false),
        new("Alias stress 48k to 16k mono", 48_000, 16_000, 1, 1.0, FidelitySignalProfile.AliasStress, FidelityEvaluationMode.OneWayReference, Gated: false)
    ];

    /// <summary>Deterministic real-audio-like fixtures used before promoting an algorithm.</summary>
    public static IReadOnlyList<FidelityScenario> RealLikeFixtures { get; } =
    [
        new("Fixture speech 48k to 16k mono", 48_000, 16_000, 1, 1.25, FidelitySignalProfile.SpeechFixture, Gated: false),
        new("Fixture speech 44.1k to 24k mono", 44_100, 24_000, 1, 1.25, FidelitySignalProfile.SpeechFixture, Gated: false),
        new("Fixture music 48k to 44.1k stereo", 48_000, 44_100, 2, 1.25, FidelitySignalProfile.MusicFixture, Gated: false),
        new("Fixture music 96k to 48k stereo", 96_000, 48_000, 2, 0.75, FidelitySignalProfile.MusicFixture, Gated: false),
        new("Fixture percussion 44.1k to 48k stereo", 44_100, 48_000, 2, 1.0, FidelitySignalProfile.PercussionFixture, Gated: false),
        new("Fixture field 48k to 32k stereo", 48_000, 32_000, 2, 1.0, FidelitySignalProfile.FieldRecordingFixture, Gated: false),
        new("Fixture high-frequency 48k to 16k mono", 48_000, 16_000, 1, 1.0, FidelitySignalProfile.HighFrequencyFixture, FidelityEvaluationMode.OneWayReference, Gated: false)
    ];

    /// <summary>All built-in deterministic fidelity scenarios.</summary>
    public static IReadOnlyList<FidelityScenario> All { get; } =
        Defaults.Concat(RealLikeFixtures).ToArray();
}
