namespace Transformations.Audio.Channels;

/// <summary>
/// Converts interleaved audio from one channel layout to mono or stereo.
/// </summary>
public sealed class Downmixer
{
    private const double InvSqrt2 = 0.7071067811865475244;
    private const double LfeFoldGain = 0.31622776601683794;
    private const double SurroundAttenuation = 0.5;

    private readonly double[] matrix;
    private readonly IReadOnlyList<double> readOnlyMatrix;

    /// <summary>
    /// Creates a downmixer with a precomputed coefficient matrix.
    /// </summary>
    /// <param name="source">The source channel layout.</param>
    /// <param name="target">The target layout. Only mono and stereo are supported.</param>
    /// <param name="coefficientSet">The coefficient set to use.</param>
    /// <exception cref="ArgumentException">Thrown when the source is empty or the target is not mono or stereo.</exception>
    /// <exception cref="ArgumentNullException">Thrown when a layout is <see langword="null"/>.</exception>
    public Downmixer(
        ChannelLayout source,
        ChannelLayout target,
        DownmixCoefficientSet coefficientSet = DownmixCoefficientSet.ItuRbs775)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        if (source.ChannelCount == 0)
        {
            throw new ArgumentException("Source layout must contain at least one channel.", nameof(source));
        }

        if (target.ChannelCount is not (1 or 2))
        {
            throw new ArgumentException("Target layout must be mono or stereo.", nameof(target));
        }

        Source = source;
        Target = target;
        CoefficientSet = coefficientSet;
        matrix = BuildMatrix(source, target.ChannelCount, coefficientSet);
        readOnlyMatrix = Array.AsReadOnly(matrix);
    }

    /// <summary>
    /// Gets the source channel layout.
    /// </summary>
    public ChannelLayout Source { get; }

    /// <summary>
    /// Gets the target channel layout.
    /// </summary>
    public ChannelLayout Target { get; }

    /// <summary>
    /// Gets the coefficient set used by this downmixer.
    /// </summary>
    public DownmixCoefficientSet CoefficientSet { get; }

    /// <summary>
    /// Gets the source channel count.
    /// </summary>
    public int SourceChannels => Source.ChannelCount;

    /// <summary>
    /// Gets the target channel count.
    /// </summary>
    public int TargetChannels => Target.ChannelCount;

    /// <summary>
    /// Gets the row-major coefficient matrix, ordered as target channel by source channel.
    /// </summary>
    public IReadOnlyList<double> Matrix => readOnlyMatrix;

    /// <summary>
    /// Calculates the number of output samples produced from an input sample count.
    /// </summary>
    /// <param name="inputSampleCount">The source sample count.</param>
    /// <returns>The target sample count.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="inputSampleCount"/> is not frame-aligned.</exception>
    public int GetOutputSampleCount(int inputSampleCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(inputSampleCount);
        if (inputSampleCount % SourceChannels != 0)
        {
            throw new ArgumentException("Input sample count must be a whole number of source frames.", nameof(inputSampleCount));
        }

        return inputSampleCount / SourceChannels * TargetChannels;
    }

    /// <summary>
    /// Downmixes interleaved samples into a newly allocated output buffer.
    /// </summary>
    /// <param name="input">The source interleaved samples.</param>
    /// <returns>The downmixed interleaved samples.</returns>
    public float[] Process(ReadOnlySpan<float> input)
    {
        var output = new float[GetOutputSampleCount(input.Length)];
        ProcessInto(input, output);
        return output;
    }

    /// <summary>
    /// Downmixes interleaved samples into a caller-provided output buffer.
    /// </summary>
    /// <param name="input">The source interleaved samples.</param>
    /// <param name="output">The destination buffer.</param>
    /// <returns>The number of frames written.</returns>
    /// <exception cref="ArgumentException">Thrown when input is not frame-aligned.</exception>
    /// <exception cref="ArgumentException">Thrown when output is too small.</exception>
    public int ProcessInto(ReadOnlySpan<float> input, Span<float> output)
    {
        var outputSampleCount = GetOutputSampleCount(input.Length);
        if (output.Length < outputSampleCount)
        {
            throw new ArgumentException($"Output buffer is too small. Need {outputSampleCount} samples, got {output.Length}.", nameof(output));
        }

        var frames = input.Length / SourceChannels;
        for (var frame = 0; frame < frames; frame++)
        {
            var inputOffset = frame * SourceChannels;
            var outputOffset = frame * TargetChannels;

            for (var targetChannel = 0; targetChannel < TargetChannels; targetChannel++)
            {
                var rowOffset = targetChannel * SourceChannels;
                double value = 0;
                for (var sourceChannel = 0; sourceChannel < SourceChannels; sourceChannel++)
                {
                    value += matrix[rowOffset + sourceChannel] * input[inputOffset + sourceChannel];
                }

                output[outputOffset + targetChannel] = (float)value;
            }
        }

        return frames;
    }

    private static double[] BuildMatrix(ChannelLayout source, int targetChannels, DownmixCoefficientSet set)
    {
        var sourceChannels = source.ChannelCount;
        var matrix = new double[targetChannels * sourceChannels];

        for (var i = 0; i < source.Positions.Count; i++)
        {
            var (left, right) = GetStereoGains(source.Positions[i], set);
            if (targetChannels == 2)
            {
                matrix[i] = left;
                matrix[sourceChannels + i] = right;
            }
            else
            {
                matrix[i] = (left + right) * InvSqrt2;
            }
        }

        if (set == DownmixCoefficientSet.AtscA85)
        {
            NormalizeHeadroom(matrix, targetChannels, sourceChannels);
        }

        return matrix;
    }

    private static (double Left, double Right) GetStereoGains(ChannelPosition position, DownmixCoefficientSet set)
        => set switch
        {
            DownmixCoefficientSet.ItuRbs775 => position switch
            {
                ChannelPosition.FrontLeft => (1, 0),
                ChannelPosition.FrontRight => (0, 1),
                ChannelPosition.FrontCenter => (InvSqrt2, InvSqrt2),
                ChannelPosition.LowFrequency => (0, 0),
                ChannelPosition.RearLeft or ChannelPosition.SideLeft => (InvSqrt2, 0),
                ChannelPosition.RearRight or ChannelPosition.SideRight => (0, InvSqrt2),
                ChannelPosition.FrontLeftCenter => (InvSqrt2, 0),
                ChannelPosition.FrontRightCenter => (0, InvSqrt2),
                ChannelPosition.RearCenter => (0.5, 0.5),
                _ => (0, 0)
            },
            DownmixCoefficientSet.AtscA85 => position switch
            {
                ChannelPosition.FrontLeft => (1, 0),
                ChannelPosition.FrontRight => (0, 1),
                ChannelPosition.FrontCenter => (InvSqrt2, InvSqrt2),
                ChannelPosition.LowFrequency => (LfeFoldGain, LfeFoldGain),
                ChannelPosition.RearLeft or ChannelPosition.SideLeft => (SurroundAttenuation, 0),
                ChannelPosition.RearRight or ChannelPosition.SideRight => (0, SurroundAttenuation),
                ChannelPosition.FrontLeftCenter => (SurroundAttenuation, 0),
                ChannelPosition.FrontRightCenter => (0, SurroundAttenuation),
                ChannelPosition.RearCenter => (SurroundAttenuation * InvSqrt2, SurroundAttenuation * InvSqrt2),
                _ => (0, 0)
            },
            _ => throw new ArgumentOutOfRangeException(nameof(set), set, "Unknown downmix coefficient set.")
        };

    private static void NormalizeHeadroom(double[] matrix, int targetChannels, int sourceChannels)
    {
        double maxRowSum = 0;
        for (var targetChannel = 0; targetChannel < targetChannels; targetChannel++)
        {
            double rowSum = 0;
            var rowOffset = targetChannel * sourceChannels;
            for (var sourceChannel = 0; sourceChannel < sourceChannels; sourceChannel++)
            {
                rowSum += Math.Abs(matrix[rowOffset + sourceChannel]);
            }

            maxRowSum = Math.Max(maxRowSum, rowSum);
        }

        if (maxRowSum <= 1)
        {
            return;
        }

        var gain = 1 / maxRowSum;
        for (var i = 0; i < matrix.Length; i++)
        {
            matrix[i] *= gain;
        }
    }
}
