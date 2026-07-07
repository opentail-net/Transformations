namespace Transformations.Audio.Analysis;

/// <summary>
/// Streaming 4x FIR true-peak detector for one audio channel.
/// </summary>
public sealed class TruePeakDetector
{
    private const int TruePeakPhases = 4;
    private const int TruePeakFirTaps = 49;
    private const int TruePeakDelay = 13;
    private const int TruePeakHistoryLength = TruePeakDelay * 2;
    private const int TruePeakInterSampleTaps = TruePeakDelay - 1;

    private static readonly double[][] InterSampleCoefficients = CreateInterSampleCoefficients();

    private readonly double[] history = new double[TruePeakHistoryLength];
    private int writePosition;

    /// <summary>
    /// Gets the maximum true peak measured so far.
    /// </summary>
    public double MaxTruePeak { get; private set; }

    /// <summary>
    /// Gets the maximum true peak measured so far in dBTP.
    /// </summary>
    public double MaxTruePeakDbTp => AudioLevels.LinearToDb(MaxTruePeak);

    /// <summary>
    /// Processes contiguous samples from one channel.
    /// </summary>
    /// <param name="samples">The samples to process.</param>
    public void Process(ReadOnlySpan<float> samples)
    {
        for (var i = 0; i < samples.Length; i++)
        {
            ProcessSample(samples[i]);
        }
    }

    /// <summary>
    /// Processes one channel from an interleaved buffer.
    /// </summary>
    /// <param name="samples">The interleaved samples.</param>
    /// <param name="offset">The channel offset within each frame.</param>
    /// <param name="stride">The channel count / frame stride.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when offset or stride are invalid.</exception>
    public void ProcessStrided(ReadOnlySpan<float> samples, int offset, int stride)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(stride, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        if (offset >= stride)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset must be less than stride.");
        }

        for (var i = offset; i < samples.Length; i += stride)
        {
            ProcessSample(samples[i]);
        }
    }

    /// <summary>
    /// Resets detector history and peak state.
    /// </summary>
    public void Reset()
    {
        Array.Clear(history);
        writePosition = 0;
        MaxTruePeak = 0;
    }

    private void ProcessSample(double sample)
    {
        MaxTruePeak = Math.Max(MaxTruePeak, MeasureSampleInterPeak(sample));
    }

    private double MeasureSampleInterPeak(double sample)
    {
        history[writePosition] = sample;
        history[writePosition + TruePeakDelay] = sample;

        var dotBase = writePosition + TruePeakDelay - 11;
        var phase1 = Dot12(dotBase, InterSampleCoefficients[0]);
        var phase2 = Dot12(dotBase, InterSampleCoefficients[1]);
        var phase3 = Dot12(dotBase, InterSampleCoefficients[2]);

        writePosition++;
        if (writePosition == TruePeakDelay)
        {
            writePosition = 0;
        }

        return Math.Max(
            Math.Abs(sample),
            Math.Max(Math.Abs(phase1), Math.Max(Math.Abs(phase2), Math.Abs(phase3))));
    }

    private double Dot12(int historyOffset, double[] coefficients)
        => history[historyOffset + 11] * coefficients[0]
            + history[historyOffset + 10] * coefficients[1]
            + history[historyOffset + 9] * coefficients[2]
            + history[historyOffset + 8] * coefficients[3]
            + history[historyOffset + 7] * coefficients[4]
            + history[historyOffset + 6] * coefficients[5]
            + history[historyOffset + 5] * coefficients[6]
            + history[historyOffset + 4] * coefficients[7]
            + history[historyOffset + 3] * coefficients[8]
            + history[historyOffset + 2] * coefficients[9]
            + history[historyOffset + 1] * coefficients[10]
            + history[historyOffset] * coefficients[11];

    private static double[][] CreateInterSampleCoefficients()
    {
        var coefficients = new[]
        {
            new double[TruePeakInterSampleTaps],
            new double[TruePeakInterSampleTaps],
            new double[TruePeakInterSampleTaps]
        };
        var center = (TruePeakFirTaps - 1) * 0.5;

        for (var tapIndex = 0; tapIndex < TruePeakFirTaps; tapIndex++)
        {
            var phase = tapIndex % TruePeakPhases;
            if (phase == 0)
            {
                continue;
            }

            var position = tapIndex - center;
            var window = 0.5 * (1 - Math.Cos(2 * Math.PI * tapIndex / (TruePeakFirTaps - 1)));
            var coefficient = Sinc(position / TruePeakPhases) * window;
            if (Math.Abs(coefficient) > 1e-12)
            {
                coefficients[phase - 1][tapIndex / TruePeakPhases] = coefficient;
            }
        }

        return coefficients;
    }

    private static double Sinc(double x)
    {
        if (Math.Abs(x) < 1e-12)
        {
            return 1;
        }

        var pix = Math.PI * x;
        return Math.Sin(pix) / pix;
    }
}
