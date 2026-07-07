namespace Transformations.Audio.Resampler.Experimental;

/// <summary>
/// Resampy/libresample-style right-wing windowed-sinc lookup with per-tap linear interpolation.
/// </summary>
/// <remarks>
/// This is intentionally a separate A/B candidate. It differs from the retired phase-table
/// candidates by storing one half-window and interpolating each tap lookup independently,
/// rather than blending prebuilt whole kernels.
/// </remarks>
public static class RightWingSinc
{
    private const int NumZeros = 64;
    private const int PrecisionBits = 10;
    private const double Rolloff = 0.917347;
    private const int TableSize = 1 << PrecisionBits;

    private static readonly Lazy<Table> Lookup = new(BuildTable, isThreadSafe: true);

    public static float[] Resample(float[] inputData, int inRate, int outRate, int channels)
    {
        if (inRate <= 0 || outRate <= 0)
            throw new ArgumentException("Sample rates must be positive.");
        if (channels <= 0)
            throw new ArgumentException("Channel count must be positive.");
        if (inputData is null || inputData.Length == 0)
            throw new ArgumentException("Input data cannot be null or empty.");
        if (inputData.Length % channels != 0)
            throw new ArgumentException("Input data length must be divisible by the number of channels.");
        if (inRate == outRate)
            return (float[])inputData.Clone();

        int inputFrames = inputData.Length / channels;
        int outputFrames = (int)Math.Round(inputFrames * (double)outRate / inRate);
        var output = new float[outputFrames * channels];
        var table = Lookup.Value;

        double sampleRatio = inRate / (double)outRate;
        double filterScale = Math.Min(1.0, outRate / (double)inRate);
        double tableStep = filterScale * TableSize;

        Parallel.For(0, outputFrames, outputFrame =>
        {
            double sourcePosition = outputFrame * sampleRatio;
            int center = (int)Math.Floor(sourcePosition);
            double frac = sourcePosition - center;

            for (int channel = 0; channel < channels; channel++)
            {
                double sum = 0;
                double weightSum = 0;

                AccumulateWing(inputData, channels, inputFrames, channel, center, -1, frac * tableStep, tableStep, table, ref sum, ref weightSum);
                AccumulateWing(inputData, channels, inputFrames, channel, center + 1, 1, (1.0 - frac) * tableStep, tableStep, table, ref sum, ref weightSum);

                output[outputFrame * channels + channel] = weightSum == 0 ? 0 : (float)(sum / weightSum);
            }
        });

        return output;
    }

    private static void AccumulateWing(
        float[] input,
        int channels,
        int inputFrames,
        int channel,
        int startFrame,
        int direction,
        double offset,
        double tableStep,
        Table table,
        ref double sum,
        ref double weightSum)
    {
        int tap = 0;
        int frame = startFrame;
        while (frame >= 0 && frame < inputFrames)
        {
            double position = offset + tap * tableStep;
            int index = (int)position;
            if (index >= table.Values.Length - 1)
                break;

            double eta = position - index;
            double weight = table.Values[index] + eta * table.Deltas[index];
            sum += input[frame * channels + channel] * weight;
            weightSum += weight;

            tap++;
            frame += direction;
        }
    }

    private static Table BuildTable()
    {
        int count = NumZeros * TableSize + 1;
        var values = new double[count];
        var deltas = new double[count];

        for (int i = 0; i < count; i++)
        {
            double x = i / (double)TableSize;
            double sinc = Rolloff * Sinc(Rolloff * x);
            double window = HannRightWing(i, count);
            values[i] = sinc * window;
        }

        for (int i = 0; i < count - 1; i++)
            deltas[i] = values[i + 1] - values[i];

        return new Table(values, deltas);
    }

    private static double HannRightWing(int index, int count)
    {
        int fullLength = 2 * (count - 1) + 1;
        int fullIndex = count - 1 + index;
        return 0.5 - 0.5 * Math.Cos(2 * Math.PI * fullIndex / (fullLength - 1));
    }

    private static double Sinc(double x)
    {
        if (Math.Abs(x) < 1e-12)
            return 1.0;

        double angle = Math.PI * x;
        return Math.Sin(angle) / angle;
    }

    private sealed record Table(double[] Values, double[] Deltas);
}
