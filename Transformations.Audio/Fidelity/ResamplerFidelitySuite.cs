using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Transformations.Audio.Resampler;
using Transformations.Audio.Resampler.Experimental;

namespace Transformations.Audio.Fidelity;

/// <summary>
/// Repeatable fidelity evidence for resamplers. The suite generates deterministic test
/// signals, performs a one-way conversion and a round-trip conversion, then measures
/// length accuracy, MSE, SNR, PSNR, and elapsed time.
/// </summary>
public static class ResamplerFidelitySuite
{
    private sealed record AlgorithmSpec(
        string Id,
        string Name,
        string Version,
        bool Experimental,
        Func<float[], int, int, int, float[]> Resample);

    private static readonly AlgorithmSpec[] Algorithms =
    [
        new("linear", "Linear", "v1", false, Resampler.Linear.Resample),
        // spline/v2: promoted (2026-07-07) from the natural_cubic_spline experimental
        // candidate - beat the old v1 Catmull-Rom implementation by 3-13 dB SNR on every
        // gated scenario. Old v1 preserved at Resampler/Experimental/Rejected/CatmullRomSpline.cs.old.
        new("spline", "Spline", "v2", false, Resampler.Spline.Resample),
        new("sinc", "Sinc", "v1", false, Resampler.Sinc.Resample),
        // sinc_v2/sinc_v3 promoted to non-experimental (2026-07-07): both are genuine,
        // durable alias-suppression alternatives to plain sinc/v1 (which has zero
        // anti-alias protection - 0 dB SNR whenever a scenario stresses aliasing), not
        // short-lived A/B experiments. sinc/v1 is kept as-is (not demoted): it still wins
        // every currently-gated scenario by large margins whenever the signal has no
        // energy that would fold at the new Nyquist.
        new("sinc_v2", "Sinc", "v2", false, JuliusSinc.Resample),
        new("nuttall_sinc", "NuttallSinc", "v1", false, NuttallSinc.Resample),
        // sinc_v3 (KaiserSinc / v3) is retired (2026-07-07): strictly dominated by nuttall_sinc
        // on both alias suppression (+0.55 dB SNR on Alias Stress) and speed (24 ms vs 26 ms),
        // plus nuttall_sinc uses a simpler 4-term cosine-sum window evaluation instead of
        // KaiserSinc's modified Bessel function (I0) power-series. Moved to
        // Resampler/Experimental/Rejected/KaiserSinc.cs.old.
        new("right_wing_sinc", "RightWingSinc", "v1", true, RightWingSinc.Resample),
        // sinc_v4 through sinc_v7 are all retired (see
        // Resampler/Experimental/Rejected/README.md): sinc_v6 was a rejected minimum-phase
        // candidate; sinc_v5 was superseded by sinc_v7; sinc_v4 never distinctly beat
        // sinc_v3 anywhere; sinc_v7 in turn never won outright against the live sinc
        // family (sinc/sinc_v2/sinc_v3) and was the clear worst of them specifically on
        // alias suppression - "better than something already retired" wasn't enough to
        // keep it once checked against the live field it actually has to compete in.
        // These IDs stay out of circulation so old report artifacts are never confused
        // with a different idea reusing the same ID.
        new("halfband_cascade", "HalfBandCascade", "v1", true, HalfBandCascade.Resample),
        new("hybrid", "Hybrid", "v1", false, Resampler.Hybrid.Resample),
        // hybrid_v2: challenger that gates on direction (upsample -> sinc/v1, downsample ->
        // sinc_v2) instead of hybrid/v1's absolute-rate gate. Retired after
        // ratio_aware_sinc_hybrid kept its major-downsample wins while recovering plain
        // sinc/v1's near-unity downsample wins.
        // hybrid_v3 is also retired (see Resampler/Experimental/Rejected/README.md).
        new("wdl_sinc", "Experimental WDL_Sinc", "v1", true, WDL_Sinc.Resample),
        // predictive and hermite are retired (see Resampler/Experimental/Rejected/README.md):
        // a weighted fidelity+speed composite audit found both strictly dominated by
        // nearest_neighbor on both axes, with no scenario where either was outright best.
        new("lanczos", "Experimental Lanczos", "v1", true, Lanczos.Resample),
        new("cubic", "Experimental Cubic", "v1", true, CubicResamplerEntryPoint.Resample),
        // Mined from wave-resampler's interpolation + LPF split: cheap zero-phase
        // Butterworth low-pass around local cubic interpolation, distinct from the
        // windowed-sinc family and worth measuring as a speed/quality middle route.
        new("zero_phase_iir_cubic", "Experimental ZeroPhaseIirCubic", "v1", true, ZeroPhaseIirCubic.Resample),
        new("nearest_neighbor", "Experimental NearestNeighbor", "v1", true, NearestNeighbor.Resample),
        // Mined idea: separates anti-alias low-pass filtering from fractional-delay
        // interpolation (Lagrange polynomial) instead of using one combined windowed-sinc
        // kernel. See Resampler/Experimental/LagrangeFractionalDelay.cs for rationale.
        new("lagrange_fd", "Experimental LagrangeFractionalDelay", "v1", true, LagrangeFractionalDelay.Resample),
        new("lagrange_fd_o6", "Experimental LagrangeFractionalDelay", "v1-o6", true, (input, inRate, outRate, channels) => LagrangeFractionalDelay.Resample(input, inRate, outRate, channels, 6)),
        new("lagrange_fd_o8", "Experimental LagrangeFractionalDelay", "v1-o8", true, (input, inRate, outRate, channels) => LagrangeFractionalDelay.Resample(input, inRate, outRate, channels, 8)),
        new("lagrange_fd_o6_r090", "Experimental LagrangeFractionalDelay", "v1-o6-r090", true, (input, inRate, outRate, channels) => LagrangeFractionalDelay.Resample(input, inRate, outRate, channels, 6, 0.90, 32)),
        new("lagrange_fd_o6_r098", "Experimental LagrangeFractionalDelay", "v1-o6-r098", true, (input, inRate, outRate, channels) => LagrangeFractionalDelay.Resample(input, inRate, outRate, channels, 6, 0.98, 32)),
        new("lagrange_fd_o6_w48", "Experimental LagrangeFractionalDelay", "v1-o6-w48", true, (input, inRate, outRate, channels) => LagrangeFractionalDelay.Resample(input, inRate, outRate, channels, 6, 0.95, 48)),
        new("lagrange_fd_o6_r098_w48", "Experimental LagrangeFractionalDelay", "v1-o6-r098-w48", true, (input, inRate, outRate, channels) => LagrangeFractionalDelay.Resample(input, inRate, outRate, channels, 6, 0.98, 48)),
        // filter_bank_sinc, rubato_table_sinc, and adaptive_kaiser_sinc are retired (see
        // Resampler/Experimental/Rejected/README.md): they mined real quarry ideas, but none
        // beat the live sinc/hybrid field strongly enough to justify a suite slot.
        // Router experiment: ratio-aware sinc everywhere except exact 2:1 downsampling,
        // where Lagrange order 6 showed a real-like music win with lower elapsed time.
        new("ratio_aware_lagrange_hybrid", "Experimental RatioAwareLagrangeHybrid", "v1", true, RatioAwareLagrangeHybrid.Resample),
        // ratio_aware_sinc_hybrid (v1) is retired (see Resampler/Experimental/Rejected/README.md):
        // superseded by the superior multi-stage hybrid router sinc_v10 (SincV10).
        // sinc_v10 (v1) is retired (2026-07-07) - strictly dominated by the newer hybrids
        // sinc_v14 and sinc_v15 on speed, and beaten by sinc_v15 on quality. Moved to
        // Resampler/Experimental/Rejected/SincV10.cs.old.
        // sinc_v11 (v1) is retired (2026-07-07) - strictly dominated by sinc_v12 (order 5 Lanczos)
        // on both quality and speed. Moved to Resampler/Experimental/Rejected/SincV11.cs.old.
        // sinc_v12 (v1) is retired (2026-07-07) - strictly dominated by sinc_v13 (order 6 Lanczos
        // + Farrow) on both quality and speed. Moved to Resampler/Experimental/Rejected/SincV12.cs.old.
        // sinc_v13 (v1) is retired (2026-07-07) - strictly dominated by sinc_v20 on both
        // quality (+0.35 dB SNR on Speech) and speed (5x faster, 6 ms vs 31 ms on Speech).
        // Moved to Resampler/Experimental/Rejected/SincV13.cs.old.
        // sinc_v14 (v1) is retired (2026-07-07) - strictly dominated by sinc_v18 on both quality
        // and speed. Moved to Resampler/Experimental/Rejected/SincV14.cs.old.
        // sinc_v15 (v1) is retired (2026-07-07) - strictly dominated by sinc_v20. Moved to
        // Resampler/Experimental/Rejected/SincV15.cs.old.
        // sinc_v16 (v1) is retired (2026-07-07) - strictly dominated by sinc_v20. Moved to
        // Resampler/Experimental/Rejected/SincV16.cs.old.
        // sinc_v17 (v1) is retired (2026-07-07) - strictly dominated by sinc_v20. Moved to
        // Resampler/Experimental/Rejected/SincV17.cs.old.
        new("sinc_v18", "Experimental SincV18", "v1", true, SincV18.Resample),
        // sinc_v19 (v1) is retired (2026-07-07) - failed strict anti-alias gating (gained only
        // 23.83 dB on Alias Stress). Moved to Resampler/Experimental/Rejected/SincV19.cs.old.
        new("sinc_v20", "Experimental SincV20", "v1", true, SincV20.Resample),
        new("cubic_farrow", "Experimental CubicFarrow", "v1", true, FarrowResampler.Resample),
        // bspline (v1), tcb_spline (v1), newton_poly (v1), and nuttall_farrow (v1) are retired
        // (see Resampler/Experimental/Rejected/README.md): bspline, tcb_spline, and newton_poly
        // failed multiple gated scenarios due to lack of stop-band filter sharpness;
        // nuttall_farrow was strictly dominated by cubic_farrow (Kaiser window fit).
        // thiran_iir (v1) is retired (see Resampler/Experimental/Rejected/README.md):
        // failed every gated scenario (getting negative SNR around -1.3 to -2.1 dB) due to
        // IIR phase dispersion (frequency-dependent group delay) which decorrelates the
        // signal from the reference. Moved to Rejected/ThiranResampler.cs.old.
        // polyphase_streaming_sinc (sinc_v8 / v8-v2) is retired (see
        // Resampler/Experimental/Rejected/README.md): matched sinc_v3's quality (tied SNR to 2
        // decimal places) but retired because it is slower (175ms vs 51ms) and not better for
        // offline whole-buffer use cases. Moved to Rejected/PolyphaseStreamingSinc.cs.old.
        // polyphase and wdl_native are retired (see Resampler/Experimental/Rejected/README.md):
        // both ported from this repo's own _previous/TestAudioUI prototype code. polyphase
        // (cached per-phase windowed-sinc filter bank) was strictly dominated by every live
        // sinc variant even after fixing two real bugs found during porting. wdl_native
        // (wrapping NAudio's real WdlResamplingSampleProvider) had a capacity-overflow bug
        // fixed, but stayed suspiciously poor - retired as "not usable as wired" rather than
        // as a verdict on the underlying WDL algorithm.
    ];

    /// <summary>Stable registry of algorithms known to the fidelity suite.</summary>
    public static IReadOnlyList<ResamplerAlgorithmInfo> AvailableAlgorithms { get; } =
        Algorithms.Select(a => new ResamplerAlgorithmInfo(a.Id, a.Name, a.Version, a.Experimental)).ToArray();

    /// <summary>Runs the default scenarios using default thresholds.</summary>
    public static ResamplerFidelityReport RunDefault()
        => Run(FidelityScenario.Defaults, ResamplerFidelityOptions.Default);

    /// <summary>Runs the supplied scenarios using the supplied thresholds.</summary>
    public static ResamplerFidelityReport Run(
        IEnumerable<FidelityScenario> scenarios,
        ResamplerFidelityOptions? options = null)
        => RunAlgorithms(scenarios, Algorithms, options);

    /// <summary>Runs ratio-aware hybrid threshold variants for tuning evidence.</summary>
    public static ResamplerFidelityReport RunRatioAwareThresholdSweep(
        IEnumerable<FidelityScenario> scenarios,
        IEnumerable<double> thresholds,
        ResamplerFidelityOptions? options = null)
    {
        var algorithms = thresholds
            .Distinct()
            .Order()
            .Select(threshold =>
            {
                string thresholdText = threshold.ToString("0.000", CultureInfo.InvariantCulture);
                string id = "ratio_aware_threshold_" + thresholdText.Replace(".", "", StringComparison.Ordinal);
                return new AlgorithmSpec(
                    id,
                    "Experimental SincV20",
                    "v1",
                    true,
                    (input, inRate, outRate, channels) => SincV20.Resample(input, inRate, outRate, channels, threshold));
            })
            .ToArray();

        return RunAlgorithms(scenarios, algorithms, options);
    }

    private static ResamplerFidelityReport RunAlgorithms(
        IEnumerable<FidelityScenario> scenarios,
        IEnumerable<AlgorithmSpec> algorithms,
        ResamplerFidelityOptions? options = null)
    {
        options ??= ResamplerFidelityOptions.Default;
        var results = new List<ResamplerFidelityResult>();
        var selectedAlgorithms = algorithms
            .Where(a => options.IncludeExperimental || !a.Experimental)
            .Where(a => options.AlgorithmIds is null || options.AlgorithmIds.Contains(a.Id));

        foreach (var scenario in scenarios)
        {
            var input = GenerateSignal(scenario);
            int inputFrames = input.Length / scenario.Channels;

            foreach (var algorithm in selectedAlgorithms)
            {
                results.Add(Evaluate(algorithm, scenario, input, inputFrames, options));
            }
        }

        return new ResamplerFidelityReport(DateTimeOffset.UtcNow, options, results);
    }

    /// <summary>Formats the report as deterministic JSON for machine comparison.</summary>
    public static string ToJson(ResamplerFidelityReport report)
        => JsonSerializer.Serialize(report, new JsonSerializerOptions
        {
            WriteIndented = true,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
        });

    /// <summary>Formats the report as CSV for spreadsheets and trend tracking.</summary>
    public static string ToCsv(ResamplerFidelityReport report)
    {
        var sb = new StringBuilder();
        sb.AppendLine("result,algorithm_id,algorithm_version,algorithm,experimental,scenario,profile,mode,gated,input_rate,output_rate,channels,input_frames,output_frames,expected_output_frames,round_trip_frames,mse,snr_db,psnr_db,elapsed_ms,notes");

        foreach (var r in report.Results.OrderBy(r => r.Scenario).ThenBy(r => r.AlgorithmId))
        {
            AppendCsv(sb, FormatResult(r));
            AppendCsv(sb, r.AlgorithmId);
            AppendCsv(sb, r.AlgorithmVersion);
            AppendCsv(sb, r.Algorithm);
            AppendCsv(sb, r.Experimental ? "yes" : "no");
            AppendCsv(sb, r.Scenario);
            AppendCsv(sb, r.SignalProfile);
            AppendCsv(sb, r.EvaluationMode);
            AppendCsv(sb, r.Gated ? "yes" : "no");
            AppendCsv(sb, r.InputRate.ToString(CultureInfo.InvariantCulture));
            AppendCsv(sb, r.OutputRate.ToString(CultureInfo.InvariantCulture));
            AppendCsv(sb, r.Channels.ToString(CultureInfo.InvariantCulture));
            AppendCsv(sb, r.InputFrames.ToString(CultureInfo.InvariantCulture));
            AppendCsv(sb, r.OutputFrames.ToString(CultureInfo.InvariantCulture));
            AppendCsv(sb, r.ExpectedOutputFrames.ToString(CultureInfo.InvariantCulture));
            AppendCsv(sb, r.RoundTripFrames.ToString(CultureInfo.InvariantCulture));
            AppendCsv(sb, r.MeanSquaredError.ToString("R", CultureInfo.InvariantCulture));
            AppendCsv(sb, r.SignalToNoiseRatioDb.ToString("R", CultureInfo.InvariantCulture));
            AppendCsv(sb, r.PeakSignalToNoiseRatioDb.ToString("R", CultureInfo.InvariantCulture));
            AppendCsv(sb, r.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture));
            AppendCsv(sb, r.FailureReason ?? string.Empty, endOfLine: true);
        }

        return sb.ToString();
    }

    /// <summary>Formats the report as a compact Markdown table for logs and artifacts.</summary>
    public static string ToMarkdown(ResamplerFidelityReport report)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Resampler Fidelity Report");
        sb.AppendLine();
        sb.AppendLine($"Started: {report.StartedAt:O}");
        sb.AppendLine($"Passed: {report.Passed}");
        sb.AppendLine($"Thresholds: SNR >= {report.Options.MinimumSnrDb:F2} dB, PSNR >= {report.Options.MinimumPsnrDb:F2} dB, MSE <= {report.Options.MaximumMeanSquaredError:G4}, frame error <= {report.Options.MaximumFrameCountError}");
        sb.AppendLine();
        sb.AppendLine("| Result | Algorithm ID | Version | Algorithm | Experimental | Scenario | Profile | Mode | SNR dB | PSNR dB | MSE | Frames | Expected | Round trip | Time ms | Notes |");
        sb.AppendLine("|---|---|---|---|---:|---|---|---|---:|---:|---:|---:|---:|---:|---:|---|");

        foreach (var r in report.Results.OrderBy(r => r.Scenario).ThenByDescending(r => r.SignalToNoiseRatioDb))
        {
            sb.Append("| ");
            sb.Append(FormatResult(r));
            sb.Append(" | ");
            sb.Append(Escape(r.AlgorithmId));
            sb.Append(" | ");
            sb.Append(Escape(r.AlgorithmVersion));
            sb.Append(" | ");
            sb.Append(Escape(r.Algorithm));
            sb.Append(" | ");
            sb.Append(r.Experimental ? "yes" : "no");
            sb.Append(" | ");
            sb.Append(Escape(r.Scenario));
            sb.Append(" | ");
            sb.Append(Escape(r.SignalProfile));
            sb.Append(" | ");
            sb.Append(Escape(r.EvaluationMode));
            sb.Append(" | ");
            sb.Append(r.SignalToNoiseRatioDb.ToString("F2"));
            sb.Append(" | ");
            sb.Append(r.PeakSignalToNoiseRatioDb.ToString("F2"));
            sb.Append(" | ");
            sb.Append(r.MeanSquaredError.ToString("G4"));
            sb.Append(" | ");
            sb.Append(r.OutputFrames);
            sb.Append(" | ");
            sb.Append(r.ExpectedOutputFrames);
            sb.Append(" | ");
            sb.Append(r.RoundTripFrames);
            sb.Append(" | ");
            sb.Append(r.ElapsedMilliseconds);
            sb.Append(" | ");
            sb.Append(Escape(r.FailureReason ?? string.Empty));
            sb.AppendLine(" |");
        }

        return sb.ToString();
    }

    private static ResamplerFidelityResult Evaluate(
        AlgorithmSpec algorithm,
        FidelityScenario scenario,
        float[] input,
        int inputFrames,
        ResamplerFidelityOptions options)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var output = algorithm.Resample(input, scenario.InputRate, scenario.OutputRate, scenario.Channels);
            var roundTrip = algorithm.Resample(output, scenario.OutputRate, scenario.InputRate, scenario.Channels);
            sw.Stop();

            int outputFrames = output.Length / scenario.Channels;
            int roundTripFrames = roundTrip.Length / scenario.Channels;
            int expectedOutputFrames = (int)Math.Round(inputFrames * (double)scenario.OutputRate / scenario.InputRate);
            int outputFrameError = Math.Abs(outputFrames - expectedOutputFrames);

            var comparison = scenario.EvaluationMode == FidelityEvaluationMode.OneWayReference
                ? GenerateOutputReference(scenario)
                : input;
            var measured = scenario.EvaluationMode == FidelityEvaluationMode.OneWayReference
                ? output
                : roundTrip;
            var (mse, snr, psnr) = Compare(comparison, measured);
            var failures = new List<string>();
            if (outputFrameError > options.MaximumFrameCountError)
                failures.Add($"frame error {outputFrameError}");
            if (mse > options.MaximumMeanSquaredError)
                failures.Add($"MSE {mse:G4}");
            if (snr < options.MinimumSnrDb)
                failures.Add($"SNR {snr:F2} dB");
            if (psnr < options.MinimumPsnrDb)
                failures.Add($"PSNR {psnr:F2} dB");

            bool meetsThresholds = failures.Count == 0;
            return new ResamplerFidelityResult(
                algorithm.Id,
                algorithm.Name,
                algorithm.Version,
                algorithm.Experimental,
                scenario.Name,
                scenario.Profile.ToString(),
                scenario.EvaluationMode.ToString(),
                scenario.Gated,
                scenario.InputRate,
                scenario.OutputRate,
                scenario.Channels,
                inputFrames,
                outputFrames,
                expectedOutputFrames,
                roundTripFrames,
                mse,
                snr,
                psnr,
                sw.ElapsedMilliseconds,
                !scenario.Gated || meetsThresholds,
                meetsThresholds,
                meetsThresholds ? null : string.Join("; ", failures));
        }
        catch (Exception ex)
        {
            sw.Stop();
            int expectedOutputFrames = (int)Math.Round(inputFrames * (double)scenario.OutputRate / scenario.InputRate);
            return new ResamplerFidelityResult(
                algorithm.Id,
                algorithm.Name,
                algorithm.Version,
                algorithm.Experimental,
                scenario.Name,
                scenario.Profile.ToString(),
                scenario.EvaluationMode.ToString(),
                scenario.Gated,
                scenario.InputRate,
                scenario.OutputRate,
                scenario.Channels,
                inputFrames,
                0,
                expectedOutputFrames,
                0,
                double.PositiveInfinity,
                double.NegativeInfinity,
                double.NegativeInfinity,
                sw.ElapsedMilliseconds,
                false,
                false,
                ex.GetType().Name + ": " + ex.Message);
        }
    }

    private static float[] GenerateSignal(FidelityScenario scenario)
    {
        int frames = Math.Max(1, (int)Math.Round(scenario.InputRate * scenario.DurationSeconds));
        var output = new float[frames * scenario.Channels];

        for (int frame = 0; frame < frames; frame++)
        {
            double t = frame / (double)scenario.InputRate;

            for (int channel = 0; channel < scenario.Channels; channel++)
            {
                output[frame * scenario.Channels + channel] = (float)GenerateSample(scenario, frame, channel);
            }
        }

        return output;
    }

    private static float[] GenerateOutputReference(FidelityScenario scenario)
    {
        return GenerateSignal(scenario with
        {
            InputRate = scenario.OutputRate,
            OutputRate = scenario.OutputRate
        });
    }

    private static double GenerateSample(FidelityScenario scenario, int frame, int channel)
    {
        return scenario.Profile switch
        {
            FidelitySignalProfile.FrequencySweep => GenerateFrequencySweep(scenario, frame, channel),
            FidelitySignalProfile.TransientPulse => GenerateTransientPulse(scenario, frame, channel),
            FidelitySignalProfile.DeterministicNoise => GenerateDeterministicNoise(frame, channel),
            FidelitySignalProfile.AliasStress => GenerateAliasStress(scenario, frame, channel),
            FidelitySignalProfile.SpeechFixture => GenerateSpeechFixture(scenario, frame, channel),
            FidelitySignalProfile.MusicFixture => GenerateMusicFixture(scenario, frame, channel),
            FidelitySignalProfile.PercussionFixture => GeneratePercussionFixture(scenario, frame, channel),
            FidelitySignalProfile.FieldRecordingFixture => GenerateFieldRecordingFixture(scenario, frame, channel),
            FidelitySignalProfile.HighFrequencyFixture => GenerateHighFrequencyFixture(scenario, frame, channel),
            _ => GenerateHarmonicBlend(scenario, frame, channel)
        };
    }

    private static double GenerateHarmonicBlend(FidelityScenario scenario, int frame, int channel)
    {
        double t = frame / (double)scenario.InputRate;
        double[] baseFrequencies = [220, 440, 1_000, 2_400, 3_600];
        double maxFrequency = Math.Min(scenario.InputRate, scenario.OutputRate) * 0.38;
        var frequencies = baseFrequencies.Where(f => f < maxFrequency).ToArray();
        if (frequencies.Length == 0)
            frequencies = [Math.Min(440, maxFrequency * 0.5)];

        double sample = 0;
        for (int i = 0; i < frequencies.Length; i++)
        {
            double channelOffset = 1.0 + channel * 0.015;
            sample += Math.Sin(2 * Math.PI * frequencies[i] * channelOffset * t) / frequencies.Length;
        }

        double envelope = 0.6 + 0.4 * Math.Sin(2 * Math.PI * 2.0 * t);
        return sample * envelope * 0.75;
    }

    private static double GenerateFrequencySweep(FidelityScenario scenario, int frame, int channel)
    {
        double duration = Math.Max(scenario.DurationSeconds, 1e-9);
        double t = frame / (double)scenario.InputRate;
        double progress = Math.Clamp(t / duration, 0, 1);
        double startFrequency = 80 + channel * 20;
        double endFrequency = Math.Min(scenario.InputRate, scenario.OutputRate) * 0.43;
        double frequency = startFrequency + (endFrequency - startFrequency) * progress;
        return Math.Sin(2 * Math.PI * frequency * t) * 0.7;
    }

    private static double GenerateTransientPulse(FidelityScenario scenario, int frame, int channel)
    {
        int period = Math.Max(1, scenario.InputRate / 20);
        int local = (frame + channel * 17) % period;
        if (local == 0)
            return 0.9;
        if (local < 12)
            return 0.9 * Math.Exp(-local / 2.5);

        double t = frame / (double)scenario.InputRate;
        return Math.Sin(2 * Math.PI * (330 + channel * 55) * t) * 0.08;
    }

    private static double GenerateDeterministicNoise(int frame, int channel)
    {
        uint state = (uint)(frame * 1_664_525 + channel * 1_013_904_223 + 0x9E3779B9);
        state ^= state >> 16;
        state *= 0x7FEB352D;
        state ^= state >> 15;
        state *= 0x846CA68B;
        state ^= state >> 16;
        return ((state / (double)uint.MaxValue) * 2.0 - 1.0) * 0.45;
    }

    private static double GenerateAliasStress(FidelityScenario scenario, int frame, int channel)
    {
        double t = frame / (double)scenario.InputRate;
        double outputNyquist = scenario.OutputRate / 2.0;
        double belowNyquist = Math.Max(120, outputNyquist * 0.82);
        double aboveNyquist = Math.Min(scenario.InputRate / 2.0 * 0.92, outputNyquist * 1.45);
        double channelOffset = 1.0 + channel * 0.01;
        double wanted = Math.Sin(2 * Math.PI * belowNyquist * channelOffset * t) * 0.35;
        if (scenario.InputRate == scenario.OutputRate)
            return wanted;

        double shouldBeSuppressed = Math.Sin(2 * Math.PI * aboveNyquist * channelOffset * t) * 0.35;
        return wanted + shouldBeSuppressed;
    }

    private static double GenerateSpeechFixture(FidelityScenario scenario, int frame, int channel)
    {
        double t = frame / (double)scenario.InputRate;
        double syllable = t % 0.42;
        double voicedGate = SmoothPulse(syllable, 0.03, 0.28, 0.035);
        double fricativeGate = SmoothPulse(syllable, 0.25, 0.35, 0.025);
        double plosiveGate = Math.Exp(-Math.Pow((syllable - 0.025) / 0.006, 2));
        double pitch = 118 + 18 * Math.Sin(2 * Math.PI * 0.73 * t) + 7 * Math.Sin(2 * Math.PI * 2.1 * t);
        double vowelMorph = 0.5 + 0.5 * Math.Sin(2 * Math.PI * 0.47 * t);
        double formant1 = 520 + 180 * vowelMorph;
        double formant2 = 1_150 + 620 * (1 - vowelMorph);
        double formant3 = 2_450 + 240 * Math.Sin(2 * Math.PI * 0.31 * t);

        double glottal = 0;
        for (int harmonic = 1; harmonic <= 14; harmonic++)
        {
            double frequency = pitch * harmonic;
            if (frequency > scenario.InputRate * 0.47)
                break;

            double formantWeight =
                Gaussian(frequency, formant1, 180) +
                0.8 * Gaussian(frequency, formant2, 260) +
                0.55 * Gaussian(frequency, formant3, 420);
            glottal += Math.Sin(2 * Math.PI * frequency * t) * formantWeight / harmonic;
        }

        double fricative = GenerateDeterministicNoise(frame, channel) * (0.6 + 0.4 * Math.Sin(2 * Math.PI * 4_700 * t));
        double breath = GenerateDeterministicNoise(frame / 3, channel + 11) * 0.08;
        return 0.34 * voicedGate * glottal + 0.18 * fricativeGate * fricative + 0.5 * plosiveGate * GenerateDeterministicNoise(frame, channel + 23) + breath;
    }

    private static double GenerateMusicFixture(FidelityScenario scenario, int frame, int channel)
    {
        double t = frame / (double)scenario.InputRate;
        double bar = t % 1.2;
        double beat = t % 0.3;
        double stereo = channel == 0 ? 0.985 : 1.015;
        double bassEnvelope = 0.45 + 0.55 * Math.Exp(-beat / 0.11);
        double chordEnvelope = 0.65 + 0.25 * Math.Sin(2 * Math.PI * 0.83 * t);
        double leadEnvelope = SmoothPulse(bar, 0.18, 0.95, 0.08);

        double bass = Math.Sin(2 * Math.PI * 55 * t) + 0.35 * Math.Sin(2 * Math.PI * 110 * t);
        double chord = 0;
        double[] chordFrequencies = [220, 277.18, 329.63, 440, 554.37, 659.25, 880, 1_318.5, 2_637.0];
        for (int i = 0; i < chordFrequencies.Length; i++)
            chord += Math.Sin(2 * Math.PI * chordFrequencies[i] * stereo * t + i * 0.37) / chordFrequencies.Length;

        double lead = 0;
        double leadFrequency = 660 + 55 * Math.Sin(2 * Math.PI * 1.7 * t);
        for (int harmonic = 1; harmonic <= 8; harmonic++)
            lead += Math.Sin(2 * Math.PI * leadFrequency * harmonic * stereo * t) / (harmonic * harmonic);

        return 0.24 * bassEnvelope * bass + 0.44 * chordEnvelope * chord + 0.23 * leadEnvelope * lead;
    }

    private static double GeneratePercussionFixture(FidelityScenario scenario, int frame, int channel)
    {
        double t = frame / (double)scenario.InputRate;
        double beat = t % 0.5;
        double offBeat = (t + 0.25) % 0.5;
        double kick = Math.Exp(-beat / 0.045) * Math.Sin(2 * Math.PI * (52 + 95 * Math.Exp(-beat / 0.035)) * t);
        double snare = Math.Exp(-offBeat / 0.028) * GenerateDeterministicNoise(frame, channel + 41);
        double hatPhase = t % 0.125;
        double hat = Math.Exp(-hatPhase / 0.012) * (GenerateDeterministicNoise(frame, channel + 73) + 0.3 * Math.Sin(2 * Math.PI * 8_200 * t));
        double tom = Math.Exp(-Math.Max(0, (t % 1.0) - 0.68) / 0.09) * Math.Sin(2 * Math.PI * 180 * t);
        return 0.48 * kick + 0.22 * snare + 0.12 * hat + 0.14 * tom;
    }

    private static double GenerateFieldRecordingFixture(FidelityScenario scenario, int frame, int channel)
    {
        double t = frame / (double)scenario.InputRate;
        double rumble = 0.18 * Math.Sin(2 * Math.PI * 38 * t + channel * 0.2);
        double hum = 0.08 * Math.Sin(2 * Math.PI * 60 * t) + 0.035 * Math.Sin(2 * Math.PI * 120 * t);
        double hiss = 0.14 * GenerateDeterministicNoise(frame, channel + 101);
        double wind = 0.18 * Math.Sin(2 * Math.PI * 0.21 * t) * GenerateDeterministicNoise(frame / 30, channel + 151);
        double sparseEvent = 0;
        double local = t % 0.77;
        if (local < 0.08)
            sparseEvent = Math.Exp(-local / 0.018) * Math.Sin(2 * Math.PI * (1_300 + channel * 80) * t);

        return rumble + hum + hiss + wind + 0.16 * sparseEvent;
    }

    private static double GenerateHighFrequencyFixture(FidelityScenario scenario, int frame, int channel)
    {
        double t = frame / (double)scenario.InputRate;
        double outputNyquist = scenario.OutputRate / 2.0;
        double inBand1 = Math.Max(220, outputNyquist * 0.72);
        double inBand2 = Math.Max(330, outputNyquist * 0.91);
        double aboveBand = Math.Min(scenario.InputRate * 0.47, outputNyquist * 1.34);
        double wanted = 0.24 * Math.Sin(2 * Math.PI * inBand1 * t) + 0.18 * Math.Sin(2 * Math.PI * inBand2 * t);
        if (scenario.InputRate == scenario.OutputRate)
            return wanted;

        double suppressed = 0.24 * Math.Sin(2 * Math.PI * aboveBand * t);
        double shimmer = 0.07 * GenerateDeterministicNoise(frame, channel + 197);
        return wanted + suppressed + shimmer;
    }

    private static double Gaussian(double x, double mean, double sigma)
        => Math.Exp(-0.5 * Math.Pow((x - mean) / sigma, 2));

    private static double SmoothPulse(double value, double start, double end, double edge)
    {
        double attack = SmoothStep((value - start) / edge);
        double release = 1.0 - SmoothStep((value - end) / edge);
        return Math.Clamp(attack * release, 0, 1);
    }

    private static double SmoothStep(double x)
    {
        x = Math.Clamp(x, 0, 1);
        return x * x * (3 - 2 * x);
    }

    private static (double Mse, double Snr, double Psnr) Compare(float[] original, float[] roundTrip)
    {
        int length = Math.Min(original.Length, roundTrip.Length);
        if (length == 0)
            return (double.PositiveInfinity, double.NegativeInfinity, double.NegativeInfinity);

        double signalPower = 0;
        double noisePower = 0;
        double peak = 0;

        for (int i = 0; i < length; i++)
        {
            double originalSample = original[i];
            double diff = originalSample - roundTrip[i];
            signalPower += originalSample * originalSample;
            noisePower += diff * diff;
            peak = Math.Max(peak, Math.Abs(originalSample));
        }

        double mse = noisePower / length;
        double snr = noisePower <= 0 ? double.PositiveInfinity : 10 * Math.Log10(signalPower / noisePower);
        double psnr = mse <= 0 ? double.PositiveInfinity : 20 * Math.Log10(Math.Max(peak, 1e-12) / Math.Sqrt(mse));
        return (mse, snr, psnr);
    }

    private static string Escape(string text) => text.Replace("|", "\\|", StringComparison.Ordinal);

    private static string FormatResult(ResamplerFidelityResult result)
    {
        if (result.Gated)
            return result.Passed ? "PASS" : "FAIL";

        return result.MeetsThresholds ? "INFO PASS" : "INFO";
    }

    private static void AppendCsv(StringBuilder sb, string value, bool endOfLine = false)
    {
        sb.Append('"');
        sb.Append(value.Replace("\"", "\"\"", StringComparison.Ordinal));
        sb.Append('"');
        sb.Append(endOfLine ? Environment.NewLine : ',');
    }

    private static class CubicResamplerEntryPoint
    {
        public static float[] Resample(float[] inputData, int inRate, int outRate, int channels)
            => new CubicResampler().Resample(inputData, inRate, outRate, channels);
    }
}

/// <summary>Public metadata for one versioned resampling algorithm.</summary>
public sealed record ResamplerAlgorithmInfo(
    string Id,
    string Name,
    string Version,
    bool Experimental);
