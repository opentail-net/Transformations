using Transformations.Audio.Fidelity;
using NUnit.Framework;

namespace Transformations.Audio.Tests.Fidelity;

[TestFixture]
public sealed class ResamplerFidelitySuiteTests
{
    [Test]
    public void AvailableAlgorithms_have_unique_versioned_ids()
    {
        var algorithms = ResamplerFidelitySuite.AvailableAlgorithms;

        Assert.That(algorithms, Is.Not.Empty);
        Assert.That(algorithms.Select(a => a.Id), Is.Unique);
        Assert.That(algorithms.Select(a => a.Version), Has.All.Match("v*"));
        Assert.That(algorithms, Has.Some.Matches<ResamplerAlgorithmInfo>(a => a.Id == "sinc" && a.Version == "v1"));
    }

    [Test]
    public void Run_can_filter_to_one_algorithm_id()
    {
        var scenario = new FidelityScenario(
            "Short deterministic smoke",
            48_000,
            16_000,
            1,
            0.05);

        var report = ResamplerFidelitySuite.Run(
            [scenario],
            ResamplerFidelityOptions.Default with
            {
                IncludeExperimental = true,
                AlgorithmIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "linear" }
            });

        Assert.That(report.Results, Has.Count.EqualTo(1));
        Assert.That(report.Results[0].AlgorithmId, Is.EqualTo("linear"));
        Assert.That(report.Results[0].AlgorithmVersion, Is.EqualTo("v1"));
        Assert.That(report.Results[0].SignalProfile, Is.EqualTo(nameof(FidelitySignalProfile.HarmonicBlend)));
    }

    [Test]
    public void Run_produces_repeatable_metrics_for_same_algorithm_and_scenario()
    {
        var scenario = new FidelityScenario(
            "Repeatability smoke",
            24_000,
            16_000,
            1,
            0.05,
            FidelitySignalProfile.FrequencySweep);

        var options = ResamplerFidelityOptions.Default with
        {
            AlgorithmIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "spline" }
        };

        var first = ResamplerFidelitySuite.Run([scenario], options).Results.Single();
        var second = ResamplerFidelitySuite.Run([scenario], options).Results.Single();

        Assert.That(second.OutputFrames, Is.EqualTo(first.OutputFrames));
        Assert.That(second.RoundTripFrames, Is.EqualTo(first.RoundTripFrames));
        Assert.That(second.MeanSquaredError, Is.EqualTo(first.MeanSquaredError));
        Assert.That(second.SignalToNoiseRatioDb, Is.EqualTo(first.SignalToNoiseRatioDb));
        Assert.That(second.PeakSignalToNoiseRatioDb, Is.EqualTo(first.PeakSignalToNoiseRatioDb));
    }

    [Test]
    public void RealLikeFixtures_cover_expected_audio_shapes()
    {
        var profiles = FidelityScenario.RealLikeFixtures.Select(s => s.Profile).ToArray();

        Assert.That(FidelityScenario.RealLikeFixtures, Has.Count.GreaterThanOrEqualTo(5));
        Assert.That(profiles, Does.Contain(FidelitySignalProfile.SpeechFixture));
        Assert.That(profiles, Does.Contain(FidelitySignalProfile.MusicFixture));
        Assert.That(profiles, Does.Contain(FidelitySignalProfile.PercussionFixture));
        Assert.That(profiles, Does.Contain(FidelitySignalProfile.FieldRecordingFixture));
        Assert.That(profiles, Does.Contain(FidelitySignalProfile.HighFrequencyFixture));
        Assert.That(FidelityScenario.All.Count, Is.EqualTo(FidelityScenario.Defaults.Count + FidelityScenario.RealLikeFixtures.Count));
    }

    [Test]
    public void RealLikeFixtures_produce_repeatable_metrics()
    {
        var scenario = FidelityScenario.RealLikeFixtures.First(s => s.Profile == FidelitySignalProfile.SpeechFixture) with
        {
            DurationSeconds = 0.05
        };
        var options = ResamplerFidelityOptions.Default with
        {
            AlgorithmIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "sinc_v2" }
        };

        var first = ResamplerFidelitySuite.Run([scenario], options).Results.Single();
        var second = ResamplerFidelitySuite.Run([scenario], options).Results.Single();

        Assert.That(first.OutputFrames, Is.GreaterThan(0));
        Assert.That(first.RoundTripFrames, Is.GreaterThan(0));
        Assert.That(second.SignalToNoiseRatioDb, Is.EqualTo(first.SignalToNoiseRatioDb));
        Assert.That(second.MeanSquaredError, Is.EqualTo(first.MeanSquaredError));
    }

    [Test]
    public void Reports_include_identity_fields_for_versioned_evidence()
    {
        var scenario = new FidelityScenario(
            "Report identity smoke",
            16_000,
            48_000,
            1,
            0.05);

        var report = ResamplerFidelitySuite.Run(
            [scenario],
            ResamplerFidelityOptions.Default with
            {
                AlgorithmIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "linear" }
            });

        string markdown = ResamplerFidelitySuite.ToMarkdown(report);
        string csv = ResamplerFidelitySuite.ToCsv(report);
        string json = ResamplerFidelitySuite.ToJson(report);

        Assert.That(markdown, Does.Contain("Algorithm ID"));
        Assert.That(markdown, Does.Contain("linear"));
        Assert.That(csv, Does.Contain("algorithm_id"));
        Assert.That(json, Does.Contain("\"AlgorithmId\": \"linear\""));
    }
}
