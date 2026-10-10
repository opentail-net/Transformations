namespace Transformations.Audio.Signals;

/// <summary>
/// Creates deterministic interleaved PCM test and utility signals.
/// </summary>
public static class AudioSignalGenerator
{
    /// <summary>
    /// Creates an interleaved sine wave.
    /// </summary>
    /// <param name="sampleRate">The sample rate in Hz.</param>
    /// <param name="frequencyHz">The sine frequency in Hz.</param>
    /// <param name="durationSeconds">The signal duration in seconds.</param>
    /// <param name="channels">The channel count.</param>
    /// <param name="amplitude">The linear amplitude.</param>
    /// <param name="phaseRadians">The starting phase in radians.</param>
    /// <returns>An interleaved PCM buffer.</returns>
    public static float[] CreateSine(
        int sampleRate,
        double frequencyHz,
        double durationSeconds,
        int channels = 1,
        double amplitude = 1,
        double phaseRadians = 0)
    {
        ValidateSignalParameters(sampleRate, durationSeconds, channels, amplitude);
        ValidateFrequency(frequencyHz, nameof(frequencyHz));
        ValidateFinite(phaseRadians, nameof(phaseRadians));

        var frames = DurationToFrames(sampleRate, durationSeconds);
        var output = new float[frames * channels];
        for (var frame = 0; frame < frames; frame++)
        {
            var sample = (float)(amplitude * Math.Sin((2 * Math.PI * frequencyHz * frame / sampleRate) + phaseRadians));
            FillFrame(output, frame, channels, sample);
        }

        return output;
    }

    /// <summary>
    /// Creates an interleaved linear frequency sweep.
    /// </summary>
    /// <param name="sampleRate">The sample rate in Hz.</param>
    /// <param name="startFrequencyHz">The starting frequency in Hz.</param>
    /// <param name="endFrequencyHz">The ending frequency in Hz.</param>
    /// <param name="durationSeconds">The signal duration in seconds.</param>
    /// <param name="channels">The channel count.</param>
    /// <param name="amplitude">The linear amplitude.</param>
    /// <returns>An interleaved PCM buffer.</returns>
    public static float[] CreateLinearChirp(
        int sampleRate,
        double startFrequencyHz,
        double endFrequencyHz,
        double durationSeconds,
        int channels = 1,
        double amplitude = 1)
    {
        ValidateSignalParameters(sampleRate, durationSeconds, channels, amplitude);
        ValidateFrequency(startFrequencyHz, nameof(startFrequencyHz));
        ValidateFrequency(endFrequencyHz, nameof(endFrequencyHz));

        var frames = DurationToFrames(sampleRate, durationSeconds);
        var output = new float[frames * channels];
        var sweepRate = durationSeconds == 0 ? 0 : (endFrequencyHz - startFrequencyHz) / durationSeconds;
        for (var frame = 0; frame < frames; frame++)
        {
            var t = (double)frame / sampleRate;
            var phase = 2 * Math.PI * ((startFrequencyHz * t) + (0.5 * sweepRate * t * t));
            var sample = (float)(amplitude * Math.Sin(phase));
            FillFrame(output, frame, channels, sample);
        }

        return output;
    }

    /// <summary>
    /// Creates an interleaved impulse signal.
    /// </summary>
    /// <param name="frames">The number of frames to create.</param>
    /// <param name="channels">The channel count.</param>
    /// <param name="impulseFrame">The frame that receives the impulse.</param>
    /// <param name="amplitude">The impulse amplitude.</param>
    /// <returns>An interleaved PCM buffer.</returns>
    public static float[] CreateImpulse(int frames, int channels = 1, int impulseFrame = 0, double amplitude = 1)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(frames, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(channels, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(impulseFrame, 0);
        ValidateFinite(amplitude, nameof(amplitude));

        if (impulseFrame >= frames)
        {
            throw new ArgumentOutOfRangeException(nameof(impulseFrame), "Impulse frame must be inside the generated signal.");
        }

        var output = new float[frames * channels];
        FillFrame(output, impulseFrame, channels, (float)amplitude);
        return output;
    }

    /// <summary>
    /// Creates deterministic interleaved white noise in the range [-amplitude, amplitude].
    /// </summary>
    /// <param name="frames">The number of frames to create.</param>
    /// <param name="channels">The channel count.</param>
    /// <param name="seed">The deterministic seed. The same parameters produce the same samples.</param>
    /// <param name="amplitude">The linear amplitude.</param>
    /// <returns>An interleaved PCM buffer.</returns>
    public static float[] CreateWhiteNoise(int frames, int channels = 1, int seed = 1, double amplitude = 1)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(frames, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(channels, 1);
        ValidateAmplitude(amplitude);

        var output = new float[frames * channels];
        var random = new XorShift32(seed == 0 ? 0x6D2B79F5u : unchecked((uint)seed));
        for (var i = 0; i < output.Length; i++)
        {
            output[i] = (float)((random.NextUnitDouble() * 2 - 1) * amplitude);
        }

        return output;
    }

    private static int DurationToFrames(int sampleRate, double durationSeconds)
        => checked((int)Math.Round(sampleRate * durationSeconds, MidpointRounding.AwayFromZero));

    private static void FillFrame(float[] output, int frame, int channels, float sample)
    {
        var offset = frame * channels;
        for (var channel = 0; channel < channels; channel++)
        {
            output[offset + channel] = sample;
        }
    }

    private static void ValidateSignalParameters(int sampleRate, double durationSeconds, int channels, double amplitude)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(sampleRate, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(channels, 1);
        if (double.IsNaN(durationSeconds) || double.IsInfinity(durationSeconds) || durationSeconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(durationSeconds), "Duration must be a finite non-negative value.");
        }

        ValidateAmplitude(amplitude);
    }

    private static void ValidateFrequency(double frequencyHz, string paramName)
    {
        if (double.IsNaN(frequencyHz) || double.IsInfinity(frequencyHz) || frequencyHz < 0)
        {
            throw new ArgumentOutOfRangeException(paramName, "Frequency must be a finite non-negative value.");
        }
    }

    private static void ValidateAmplitude(double amplitude)
    {
        if (double.IsNaN(amplitude) || double.IsInfinity(amplitude) || amplitude < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amplitude), "Amplitude must be a finite non-negative value.");
        }
    }

    private static void ValidateFinite(double value, string paramName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new ArgumentOutOfRangeException(paramName, "Value must be finite.");
        }
    }

    private sealed class XorShift32
    {
        private uint state;

        public XorShift32(uint seed)
        {
            state = seed == 0 ? 1u : seed;
        }

        public double NextUnitDouble()
        {
            var x = state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            state = x;
            return x / (uint.MaxValue + 1.0);
        }
    }
}
