using System.Runtime.CompilerServices;

namespace Transformations.Audio.Resampler.Experimental
{
    /// <summary>
    /// Experimental cubic resampler with a cheap forward/backward Butterworth low-pass
    /// prefilter for downsampling.
    /// </summary>
    /// <remarks>
    /// Quarry source: wave-resampler's LPF + interpolation split. This candidate tests a
    /// different tradeoff from the sinc family: low-cost IIR anti-alias filtering followed
    /// by local cubic interpolation. It is not linear phase internally, but the
    /// forward/backward pass cancels phase shift for whole-buffer use.
    /// </remarks>
    public static class ZeroPhaseIirCubic
    {
        private const int FilterOrder = 8;

        /// <summary>
        /// Resamples interleaved audio using local cubic interpolation, with a
        /// forward/backward Butterworth low-pass prefilter when downsampling.
        /// </summary>
        /// <param name="inputData">Interleaved audio samples.</param>
        /// <param name="inRate">Input sample rate in Hz.</param>
        /// <param name="outRate">Output sample rate in Hz.</param>
        /// <param name="channels">Number of interleaved channels.</param>
        /// <returns>Resampled interleaved audio samples.</returns>
        public static float[] Resample(float[] inputData, int inRate, int outRate, int channels)
        {
            if (inRate <= 0 || outRate <= 0)
                throw new ArgumentException("Sample rates must be positive.");
            if (channels <= 0)
                throw new ArgumentException("Channel count must be positive.");
            if (inputData == null || inputData.Length == 0)
                throw new ArgumentException("Input data cannot be null or empty.");
            if (inputData.Length % channels != 0)
                throw new ArgumentException("Input data length must be divisible by the number of channels.");

            if (inRate == outRate)
                return (float[])inputData.Clone();

            int inputFrames = inputData.Length / channels;
            int outputFrames = (int)Math.Round(inputFrames * (double)outRate / inRate);
            if (outputFrames == 0)
                return Array.Empty<float>();

            float[] source = outRate < inRate
                ? ApplyZeroPhaseLowPass(inputData, inputFrames, channels, inRate, outRate * 0.46)
                : inputData;

            return ResampleCubic(source, inputFrames, outputFrames, channels, inRate / (double)outRate);
        }

        private static float[] ApplyZeroPhaseLowPass(float[] input, int frames, int channels, int sampleRate, double cutoffHz)
        {
            var filtered = (float[])input.Clone();

            for (int channel = 0; channel < channels; channel++)
            {
                var forward = new ButterworthLowPass(FilterOrder, sampleRate, cutoffHz);
                for (int frame = 0; frame < frames; frame++)
                {
                    int index = frame * channels + channel;
                    filtered[index] = (float)forward.Filter(filtered[index]);
                }

                var backward = new ButterworthLowPass(FilterOrder, sampleRate, cutoffHz);
                for (int frame = frames - 1; frame >= 0; frame--)
                {
                    int index = frame * channels + channel;
                    filtered[index] = (float)backward.Filter(filtered[index]);
                }
            }

            return filtered;
        }

        private static float[] ResampleCubic(float[] input, int inputFrames, int outputFrames, int channels, double ratio)
        {
            var output = new float[outputFrames * channels];

            Parallel.For(0, outputFrames, frame =>
            {
                double sourcePosition = Math.Clamp(frame * ratio, 0, inputFrames - 1);
                int baseFrame = (int)sourcePosition;
                double t = sourcePosition - baseFrame;
                int outputIndex = frame * channels;

                for (int channel = 0; channel < channels; channel++)
                {
                    float y0 = GetSample(input, inputFrames, baseFrame - 1, channels, channel);
                    float y1 = GetSample(input, inputFrames, baseFrame, channels, channel);
                    float y2 = GetSample(input, inputFrames, baseFrame + 1, channels, channel);
                    float y3 = GetSample(input, inputFrames, baseFrame + 2, channels, channel);
                    output[outputIndex + channel] = CubicInterpolate(y0, y1, y2, y3, t);
                }
            });

            return output;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float GetSample(float[] data, int frames, int frame, int channels, int channel)
        {
            frame = Math.Clamp(frame, 0, frames - 1);
            return data[frame * channels + channel];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float CubicInterpolate(float y0, float y1, float y2, float y3, double t)
        {
            double a = -0.5 * y0 + 1.5 * y1 - 1.5 * y2 + 0.5 * y3;
            double b = y0 - 2.5 * y1 + 2.0 * y2 - 0.5 * y3;
            double c = -0.5 * y0 + 0.5 * y2;
            return (float)(((a * t + b) * t + c) * t + y1);
        }

        private sealed class ButterworthLowPass
        {
            private readonly BiquadStage[] stages;

            public ButterworthLowPass(int order, double sampleRate, double cutoffHz)
            {
                stages = new BiquadStage[order];
                double cutoff = Math.Clamp(cutoffHz, 1.0, sampleRate * 0.49);

                for (int i = 0; i < stages.Length; i++)
                {
                    double q = 0.5 / Math.Sin((Math.PI / (order * 2.0)) * (i + 0.5));
                    stages[i] = BiquadStage.CreateLowPass(sampleRate, cutoff, q);
                }
            }

            public double Filter(double sample)
            {
                double value = sample;
                for (int i = 0; i < stages.Length; i++)
                    value = stages[i].Filter(value);

                return value;
            }
        }

        private struct BiquadStage
        {
            private readonly double b0;
            private readonly double b1;
            private readonly double b2;
            private readonly double a1;
            private readonly double a2;
            private double z1;
            private double z2;

            private BiquadStage(double b0, double b1, double b2, double a1, double a2)
            {
                this.b0 = b0;
                this.b1 = b1;
                this.b2 = b2;
                this.a1 = a1;
                this.a2 = a2;
                z1 = 0;
                z2 = 0;
            }

            public static BiquadStage CreateLowPass(double sampleRate, double cutoffHz, double q)
            {
                double w = 2.0 * Math.PI * cutoffHz / sampleRate;
                double cos = Math.Cos(w);
                double alpha = Math.Sin(w) / (2.0 * q);
                double a0 = 1.0 + alpha;
                double b0 = (1.0 - cos) / (2.0 * a0);
                double b1 = (1.0 - cos) / a0;
                double b2 = b0;
                double a1 = (-2.0 * cos) / a0;
                double a2 = (1.0 - alpha) / a0;
                return new BiquadStage(b0, b1, b2, a1, a2);
            }

            public double Filter(double input)
            {
                double temp = input - a1 * z1 - a2 * z2;
                double output = b0 * temp + b1 * z1 + b2 * z2;
                z2 = z1;
                z1 = temp;
                return output;
            }
        }
    }
}
