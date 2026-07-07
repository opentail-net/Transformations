using NAudio.Wave;

namespace NAudio.Transformations.Resampler
{
    /// <summary>
    /// Provides a basic linear interpolation resampler for interleaved multi-channel audio.
    /// </summary>
    ///     /// <example>
    /// Real-Time Integration Potential
    /// public class LinearStreamingResampler : IStreamingResampler
    /// {
    ///     private readonly ISampleProvider _source;
    ///     // internal ring buffer, interpolation logic, etc.
    /// 
    ///     public int InputSampleRate => ...
    ///     public int OutputSampleRate => ...
    ///     public int Channels => ...
    /// 
    ///     public int Read(float[] buffer, int offset, int count)
    ///     {
    ///         // Streaming resample logic here
    ///     }
    /// }
    /// 
    /// </example>
    public class LinearResampler : IAudioResampler
    {
        /// <summary>
        /// Resamples interleaved audio data using linear interpolation, based on input and output <see cref="WaveFormat"/>s.
        /// </summary>
        /// <param name="inputData">Interleaved audio samples as a float array.</param>
        /// <param name="inFormat">The <see cref="WaveFormat"/> describing the input sample rate and channel count.</param>
        /// <param name="outFormat">The <see cref="WaveFormat"/> describing the desired output sample rate and channel count.</param>
        /// <returns>Resampled interleaved audio data as a float array.</returns>
        /// <exception cref="ArgumentNullException">Thrown if either <paramref name="inFormat"/> or <paramref name="outFormat"/> is null.</exception>
        public float[] Resample(float[] inputData, WaveFormat inFormat, WaveFormat outFormat)
        {
            if (inFormat == null || outFormat == null)
                throw new ArgumentNullException("Both input and output WaveFormat must be provided.");

            return Resample(inputData, inFormat.SampleRate, outFormat.SampleRate, inFormat.Channels);
        }

        /// <summary>
        /// Resamples audio data using linear interpolation.
        /// </summary>
        /// <param name="inputData">Interleaved audio samples.</param>
        /// <param name="inRate">Input sample rate (Hz).</param>
        /// <param name="outRate">Desired output sample rate (Hz).</param>
        /// <param name="channels">Number of audio channels (e.g. 1 for mono, 2 for stereo).</param>
        /// <returns>Resampled interleaved audio data.</returns>
        public float[] Resample(float[] inputData, int inRate, int outRate, int channels)
        {
            // Validate inputs
            if (inRate <= 0 || outRate <= 0)
                throw new ArgumentException("Sample rates must be positive.");
            if (channels <= 0)
                throw new ArgumentException("Channel count must be positive.");
            if (inputData == null || inputData.Length == 0)
                throw new ArgumentException("Input data cannot be null or empty.");
            if (inputData.Length % channels != 0)
                throw new ArgumentException("Input data length must be divisible by the number of channels.");

            // If the sample rate hasn't changed, return the original data (no resampling needed)
            if (inRate == outRate)
                return inputData;

            // Calculate the ratio of input rate to output rate.
            double ratio = (double)inRate / outRate;

            int inputFrames = inputData.Length / channels;
            int outputFrames = (int)(inputFrames / ratio); // How many frames we'll have in the output

            // Guard against producing an empty buffer
            if (outputFrames == 0)
                return Array.Empty<float>();

            float[] output = new float[outputFrames * channels];

            // Process each output frame in parallel for performance
            Parallel.For(0, outputFrames, i =>
            {
                double srcPos = i * ratio;
                int baseIndex = (int)srcPos;
                double frac = srcPos - baseIndex; // Fractional distance between baseIndex and next sample

                // If we've reached the very end, just repeat the last sample to avoid out-of-bounds access
                if (baseIndex >= inputFrames - 1)
                {
                    for (int ch = 0; ch < channels; ch++)
                        output[i * channels + ch] = inputData[(inputFrames - 1) * channels + ch];
                }
                else
                {
                    // Interpolate between baseIndex and baseIndex + 1 for each channel
                    for (int ch = 0; ch < channels; ch++)
                    {
                        int addr1 = baseIndex * channels + ch;
                        int addr2 = (baseIndex + 1) * channels + ch;

                        float s1 = inputData[addr1];
                        float s2 = inputData[addr2];

                        // Linear interpolation: (1 - frac) * s1 + frac * s2
                        output[i * channels + ch] = (float)((1.0 - frac) * s1 + frac * s2);
                    }
                }
            });

            return output;
        }
    }

    public static class Linear
    {
        public static float[] Resample(float[] inputData, int inRate, int outRate, int channels)
        {
            LinearResampler lr = new LinearResampler();
            return lr.Resample(inputData, inRate, outRate, channels);
        }

    }
}
