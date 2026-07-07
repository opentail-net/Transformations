using NAudio.Wave;
using System.Collections.Concurrent;
using System.Numerics;

namespace Transformations.Audio.Resampler
{
    /// <summary>
    /// Public interface for sinc-based audio resampling with configurable quality.
    /// </summary>
    public static class Sinc
    {
        private static readonly SincResampler resampler = new SincResampler();

        /// <summary>
        /// Resample audio using a specific kernel half-width (manually controlled).
        /// </summary>
        public static float[] Resample(float[] inputData, int inRate, int outRate, int channels, int kernelHalfWidth = 10)
        {
            return resampler.Resample(inputData, inRate, outRate, channels, kernelHalfWidth);
        }

        /// <summary>
        /// Resample audio using a predefined quality level.
        /// </summary>
        public static float[] Resample(float[] inputData, int inRate, int outRate, int channels, ResampleQuality quality)
        {
            return resampler.Resample(inputData, inRate, outRate, channels, (int)quality);
        }

        /// <summary>
        /// Internal call with default quality (Balanced = 64).
        /// </summary>
        public static float[] Resample(float[] inputData, int inRate, int outRate, int channels)
        {
            return resampler.Resample(inputData, inRate, outRate, channels, (int)ResampleQuality.Balanced);
        }
    }

    /// <summary>
    /// Abstract base for resamplers using convolution kernels.
    /// </summary>
    internal abstract class KernelResamplerBase
    {
        // Keyed on an exact reduced fraction (half-width, numerator, denominator) rather
        // than a rounded double. The fractional sample position for output index i is
        // exactly i * inRate/outRate; reduced to lowest terms via gcd, that has a bounded
        // number of distinct values (at most outRate/gcd(inRate,outRate)) and repeats
        // exactly - no floating-point drift. Computing it via repeated double division
        // and rounding to 8 decimals (the previous approach) drifted enough, for rate
        // pairs whose reduced denominator isn't a power of two (e.g. 48000:44100 reduces
        // to 160:147), that almost every output sample missed the cache and paid for a
        // full kernel rebuild - measured at ~21s for a 1s 48kHz->44.1kHz round trip vs.
        // low single-digit ms for power-of-two-friendly ratios.
        protected static readonly ConcurrentDictionary<KernelKey, Lazy<double[]>> kernelCache =
            new ConcurrentDictionary<KernelKey, Lazy<double[]>>();

        protected readonly record struct KernelKey(int HalfWidth, int Numerator, int Denominator);

        public float[] Resample(float[] inputData, int inRate, int outRate, int channels, int kernelHalfWidth)
        {
            if (inRate <= 0 || outRate <= 0)
                throw new ArgumentException("Sample rates must be positive.");
            if (channels <= 0)
                throw new ArgumentException("Channel count must be positive.");
            if (inputData == null || inputData.Length == 0)
                throw new ArgumentException("Input data cannot be null or empty.");
            if (inputData.Length % channels != 0)
                throw new ArgumentException("Input data length must be divisible by the number of channels.");

            float[][] channelData = SplitChannels(inputData, channels);
            double ratio = (double)outRate / inRate;
            int rateGcd = Gcd(inRate, outRate);
            int p = inRate / rateGcd;
            int q = outRate / rateGcd;

            Parallel.For(0, channels, c =>
            {
                channelData[c] = ResampleChannel(channelData[c], ratio, p, q, kernelHalfWidth);
            });

            return CombineChannels(channelData, channels);
        }

        protected abstract float[] ResampleChannel(float[] channel, double ratio, int p, int q, int kernelHalfWidth);
        protected abstract double ComputeKernelWeight(double x, int L);

        protected double[] ComputeKernel(double frac, int kernelHalfWidth)
        {
            // Sequential on purpose: kernel length is only 2*halfWidth (tens to a few
            // hundred taps at most), far too small for Parallel.For's scheduling overhead
            // to pay off - especially since this runs on every cache miss.
            int kernelLength = 2 * kernelHalfWidth;
            double[] weights = new double[kernelLength];

            for (int i = 0; i < kernelLength; i++)
            {
                int k = i - kernelHalfWidth + 1;
                weights[i] = ComputeKernelWeight(frac - k, kernelHalfWidth);
            }

            return weights;
        }

        protected static int Gcd(int a, int b)
        {
            a = Math.Abs(a);
            b = Math.Abs(b);
            while (b != 0)
            {
                (a, b) = (b, a % b);
            }
            return a == 0 ? 1 : a;
        }

        protected double ProcessSinc(double x)
        {
            if (x == 0)
                return 1.0;
            double pix = Math.PI * x;
            return Math.Sin(pix) / pix;
        }

        protected double HannWindow(double x, int L)
        {
            if (Math.Abs(x) > L)
                return 0.0;
            return 0.5 * (1 + Math.Cos(Math.PI * x / L));
        }

        protected float[][] SplitChannels(float[] inputData, int channels)
        {
            int frames = inputData.Length / channels;
            float[][] output = new float[channels][];
            for (int c = 0; c < channels; c++)
            {
                output[c] = new float[frames];
                for (int i = 0; i < frames; i++)
                {
                    output[c][i] = inputData[i * channels + c];
                }
            }
            return output;
        }

        protected float[] CombineChannels(float[][] channelData, int channels)
        {
            int frames = channelData[0].Length;
            float[] output = new float[frames * channels];
            for (int i = 0; i < frames; i++)
            {
                for (int c = 0; c < channels; c++)
                {
                    output[i * channels + c] = channelData[c][i];
                }
            }
            return output;
        }
    }

    /// <summary>
    /// Concrete sinc resampler using a windowed sinc kernel
    /// with SIMD optimizations in the convolution loop.
    /// </summary>
    internal class SincResampler : KernelResamplerBase, IAudioResampler
    {

        /// <summary>
        /// Resample using default (Balanced) quality.
        /// </summary>
        public float[] Resample(float[] inputData, int inRate, int outRate, int channels)
            => Resample(inputData, inRate, outRate, channels, (int)ResampleQuality.Balanced);

        /// <summary>
        /// Resample using WaveFormat inputs and default (Balanced) quality.
        /// </summary>
        public float[] Resample(float[] inputData, WaveFormat inFormat, WaveFormat outFormat)
            => Resample(inputData, inFormat.SampleRate, outFormat.SampleRate, inFormat.Channels, (int)ResampleQuality.Balanced);

        protected override double ComputeKernelWeight(double x, int L)
            => ProcessSinc(x) * HannWindow(x, L);


        //// Compute the kernel weight: sinc(x)*hann(x,L)
        //protected override double ComputeKernelWeight(double x, int L)
        //{
        //    return ProcessSinc(x) * HannWindow(x, L);
        //}

        /// <summary>
        /// Resample a single channel using windowed sinc convolution.
        /// Key optimizations:
        /// - Padded channel copy to avoid boundary checks.
        /// - Conversion of padded data to double for high-precision vectorized dot product.
        /// - Use of System.Numerics.Vector&lt;double&gt; to compute dot products in chunks.
        /// </summary>
        protected override float[] ResampleChannel(float[] channel, double ratio, int p, int q, int kernelHalfWidth)
        {
            int inputLength = channel.Length;
            int outputLength = (int)Math.Round(inputLength * ratio);
            float[] output = new float[outputLength];

            // Prepare padded channel: pad both ends so convolution does not need bounds checks.
            int pad = kernelHalfWidth + 16;
            int vectorSize = Vector<float>.Count;
            int totalPad = (pad + vectorSize - 1) / vectorSize * vectorSize;
            float[] paddedChannel = new float[inputLength + 2 * totalPad];
            Array.Copy(channel, 0, paddedChannel, totalPad, inputLength);
            for (int i = 0; i < totalPad; i++)
            {
                paddedChannel[i] = channel[0];
                paddedChannel[paddedChannel.Length - 1 - i] = channel[inputLength - 1];
            }

            // Convert paddedChannel from float to double once.
            double[] paddedChannelDouble = new double[paddedChannel.Length];
            for (int i = 0; i < paddedChannel.Length; i++)
                paddedChannelDouble[i] = paddedChannel[i];

            Parallel.For(0, outputLength, i =>
            {
                // Exact position: i/ratio == i * inRate/outRate == i * p/q (p,q reduced,
                // coprime), computed with integer arithmetic so it never drifts. rem/q
                // (further reduced by their own gcd) is the exact fractional offset used
                // both to evaluate the kernel and as the cache key.
                long num = (long)i * p;
                int n = (int)(num / q);
                int rem = (int)(num % q);

                double frac;
                KernelKey key;
                if (rem == 0)
                {
                    frac = 0.0;
                    key = new KernelKey(kernelHalfWidth, 0, 1);
                }
                else
                {
                    int g = Gcd(rem, q);
                    int redRem = rem / g;
                    int redQ = q / g;
                    frac = (double)redRem / redQ;
                    key = new KernelKey(kernelHalfWidth, redRem, redQ);
                }

                double[] kernel = kernelCache
                    .GetOrAdd(key, _ => new Lazy<double[]>(() => ComputeKernel(frac, kernelHalfWidth), true))
                    .Value;

                double sum = 0.0;
                double weightSum = 0.0;
                int kBase = -kernelHalfWidth + 1;
                int kernelLen = kernel.Length;
                int paddedOffset = n + kBase + totalPad;

                // Use vectorized dot product if we can guarantee safe access.
                bool safeToUnroll = kernelLen >= Vector<double>.Count &&
                                    paddedOffset + kernelLen <= paddedChannelDouble.Length &&
                                    paddedOffset >= 0;
                int k = 0;
                int vecSize = Vector<double>.Count; // typically 2, 4, or 8 doubles
                if (safeToUnroll)
                {
                    int unrollLimit = kernelLen - kernelLen % vecSize;
                    Vector<double> vecSum = Vector<double>.Zero;
                    Vector<double> vecWeightSum = Vector<double>.Zero;
                    for (; k < unrollLimit; k += vecSize)
                    {
                        Vector<double> vecInput = new Vector<double>(paddedChannelDouble, paddedOffset + k);
                        Vector<double> vecKernel = new Vector<double>(kernel, k);
                        vecSum += vecInput * vecKernel;
                        vecWeightSum += vecKernel;
                    }
                    for (int j = 0; j < vecSize; j++)
                    {
                        sum += vecSum[j];
                        weightSum += vecWeightSum[j];
                    }
                }
                // Process remaining kernel taps.
                for (; k < kernelLen; k++)
                {
                    int index = n + kBase + k + totalPad;
                    if (index >= 0 && index < paddedChannelDouble.Length)
                    {
                        double weight = kernel[k];
                        sum += paddedChannelDouble[index] * weight;
                        weightSum += weight;
                    }
                }

                output[i] = (float)(sum / weightSum);
            });

            return output;
        }
    }
}
