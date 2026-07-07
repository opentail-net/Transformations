using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace NAudio.Transformations.Resampler.Experimental
{
    public static class Lanczos
    {
        // Cache kernel weights keyed by (effectiveA, rounded fractional offset)
        private static readonly ConcurrentDictionary<(int effectiveA, double frac), Lazy<double[]>> kernelCache =
            new ConcurrentDictionary<(int, double), Lazy<double[]>>();

        internal static float[] Resample(float[] inputData, int inRate, int outRate, int channels)
        {
            return Resample(inputData, inRate, outRate, channels, 3);
        }

        public static float[] Resample(float[] inputData, int inRate, int outRate, int channels, int a = 3)
        {
            // Input validation
            if (inRate <= 0 || outRate <= 0)
                throw new ArgumentException("Sample rates must be positive.");
            if (channels <= 0)
                throw new ArgumentException("Channel count must be positive.");
            if (inputData == null || inputData.Length == 0)
                throw new ArgumentException("Input data cannot be null or empty.");
            if (inputData.Length % channels != 0)
                throw new ArgumentException("Input data length must be divisible by the number of channels.");
            if (inRate == outRate)
                return inputData; // No resampling needed

            // Compute resampling ratio and output size
            double ratio = (double)inRate / outRate;
            int inputFrames = inputData.Length / channels;
            int outputFrames = (int)(inputFrames / ratio);
            if (outputFrames == 0)
                return Array.Empty<float>();

            // Adaptive kernel size: increase the effective kernel half-width when downsampling.
            // This boosts quality by using a wider kernel to capture low-frequency details.
            int effectiveA = a;
            if (inRate > outRate)
            {
                effectiveA = (int)Math.Ceiling(a * ratio);
            }

            // Rent a temporary output buffer
            float[] tempOutput = ArrayPool<float>.Shared.Rent(outputFrames * channels);

            Parallel.For(0, outputFrames, i =>
            {
                double fractionalPosition = i * ratio;
                int baseIndex = (int)fractionalPosition;
                // We round the fractional part (to 8 decimal places) to cluster similar positions.
                double fracPart = Math.Round(fractionalPosition - Math.Floor(fractionalPosition), 8);
                var kernelKey = (effectiveA, fracPart);

                // Retrieve or calculate the kernel weights based on the effective kernel width and fractional offset.
                double[] kernel = kernelCache.GetOrAdd(kernelKey, key =>
                    new Lazy<double[]>(() =>
                    {
                        int len = 2 * key.effectiveA + 1;
                        double[] arr = new double[len];
                        for (int k = -key.effectiveA; k <= key.effectiveA; k++)
                        {
                            // We compute the weight for each tap: comparing the fractional offset relative to each tap.
                            arr[k + key.effectiveA] = LanczosKernel(fracPart - k, key.effectiveA);
                        }
                        return arr;
                    }, true)
                ).Value;

                int kernelLength = kernel.Length;
                for (int ch = 0; ch < channels; ch++)
                {
                    double result = 0.0;
                    double weightSum = 0.0;

                    for (int k = 0; k < kernelLength; k++)
                    {
                        // Compute the corresponding sample index and clamp it to valid bounds.
                        int sampleIndex = baseIndex + (k - effectiveA);
                        sampleIndex = sampleIndex < 0 ? 0 : (sampleIndex >= inputFrames ? inputFrames - 1 : sampleIndex);
                        double weight = kernel[k];

                        result += inputData[sampleIndex * channels + ch] * weight;
                        weightSum += weight;
                    }

                    // Write out the final value with a safeguard against division-by-zero.
                    tempOutput[i * channels + ch] = (float)(result / Math.Max(weightSum, 1e-8));
                }
            });

            // Slice the output buffer to the actual length and return it.
            float[] finalOutput = tempOutput[..(outputFrames * channels)];
            ArrayPool<float>.Shared.Return(tempOutput, clearArray: true);
            return finalOutput;
        }

        // Lanczos kernel function: sinc(x) * sinc(x / a)
        private static double LanczosKernel(double x, int a)
        {
            x = Math.Abs(x);
            if (x == 0)
                return 1;
            if (x >= a)
                return 0;

            double pix = Math.PI * x;
            return a * Math.Sin(pix) * Math.Sin(pix / a) / (pix * pix);
        }
    }
}



////////using System.Buffers;

////////namespace NAudio.Transformations.Resampler.Experimental
////////{
////////    public static class Lanczos
////////    {
////////        internal static float[] Resample(float[] inputData, int inRate, int outRate, int channels)
////////        {
////////            return Resample(inputData, inRate, outRate, channels, 3);
////////        }

////////        public static float[] Resample(float[] inputData, int inRate, int outRate, int channels, int a = 3)
////////        {
////////            // Input validation
////////            if (inRate <= 0 || outRate <= 0)
////////                throw new ArgumentException("Sample rates must be positive.");
////////            if (channels <= 0)
////////                throw new ArgumentException("Channel count must be positive.");
////////            if (inputData == null || inputData.Length == 0)
////////                throw new ArgumentException("Input data cannot be null or empty.");
////////            if (inputData.Length % channels != 0)
////////                throw new ArgumentException("Input data length must be divisible by the number of channels.");
////////            if (inRate == outRate)
////////                return inputData; // No resampling needed

////////            // Compute resampling ratio and output size
////////            double ratio = (double)inRate / outRate;
////////            int inputFrames = inputData.Length / channels;
////////            int outputFrames = (int)(inputFrames / ratio);
////////            if (outputFrames == 0)
////////                return System.Array.Empty<float>();

////////            // Adaptive kernel size: increase the effective kernel half-width when downsampling.
////////            // This boosts quality by using a wider kernel to better capture low-frequency details.
////////            int effectiveA = a;
////////            if (inRate > outRate)
////////            {
////////                effectiveA = (int)Math.Ceiling(a * ratio);
////////            }

////////            // Use ArrayPool for memory efficiency
////////            float[] tempOutput = ArrayPool<float>.Shared.Rent(outputFrames * channels);

////////            Parallel.For(0, outputFrames, i =>
////////            {
////////                double fractionalPosition = i * ratio;
////////                int baseIndex = (int)fractionalPosition;

////////                for (int ch = 0; ch < channels; ch++)
////////                {
////////                    // Use double for accumulation to enhance numerical precision.
////////                    double result = 0.0;
////////                    double weightSum = 0.0;

////////                    for (int k = -effectiveA; k <= effectiveA; k++)
////////                    {
////////                        int sampleIndex = baseIndex + k;
////////                        // Clamp the index to valid bounds.
////////                        sampleIndex = sampleIndex < 0 ? 0 : (sampleIndex >= inputFrames ? inputFrames - 1 : sampleIndex);

////////                        double distance = fractionalPosition - sampleIndex;
////////                        double weight = LanczosKernel(distance, effectiveA);

////////                        result += inputData[sampleIndex * channels + ch] * weight;
////////                        weightSum += weight;
////////                    }

////////                    // Convert accumulated result back to float.
////////                    tempOutput[i * channels + ch] = (float)(result / Math.Max(weightSum, 1e-8));
////////                }
////////            });

////////            // Slice and return final output, and return the pooled array.
////////            float[] finalOutput = tempOutput[..(outputFrames * channels)];
////////            ArrayPool<float>.Shared.Return(tempOutput, clearArray: true);
////////            return finalOutput;
////////        }

////////        // Lanczos interpolation kernel: sinc(x) * sinc(x / a)
////////        private static double LanczosKernel(double x, int a)
////////        {
////////            x = Math.Abs(x);
////////            if (x == 0)
////////                return 1;
////////            if (x >= a)
////////                return 0;

////////            double pix = Math.PI * x;
////////            return a * Math.Sin(pix) * Math.Sin(pix / a) / (pix * pix);
////////        }
////////    }
////////}




////using System.Buffers;

////namespace NAudio.Transformations.Resampler.Experimental
////{
////    public static class Lanczos
////    {
////        internal static float[] Resample(float[] inputData, int inRate, int outRate, int channels)
////        {
////            return Resample(inputData, inRate, outRate, channels, 3);
////        }

////        public static float[] Resample(float[] inputData, int inRate, int outRate, int channels, int a = 3)
////        {
////            // Input validation
////            if (inRate <= 0 || outRate <= 0)
////                throw new ArgumentException("Sample rates must be positive.");
////            if (channels <= 0)
////                throw new ArgumentException("Channel count must be positive.");
////            if (inputData == null || inputData.Length == 0)
////                throw new ArgumentException("Input data cannot be null or empty.");
////            if (inputData.Length % channels != 0)
////                throw new ArgumentException("Input data length must be divisible by the number of channels.");
////            if (inRate == outRate)
////                return inputData; // No resampling needed

////            // Compute resampling ratio and output size
////            double ratio = (double)inRate / outRate;
////            int inputFrames = inputData.Length / channels;
////            int outputFrames = (int)(inputFrames / ratio);

////            if (outputFrames == 0)
////                return Array.Empty<float>();

////            // Use ArrayPool for memory efficiency
////            float[] tempOutput = ArrayPool<float>.Shared.Rent(outputFrames * channels);

////            Parallel.For(0, outputFrames, i =>
////            {
////                double fractionalPosition = i * ratio;
////                int baseIndex = (int)fractionalPosition;

////                for (int ch = 0; ch < channels; ch++)
////                {
////                    float result = 0;
////                    float weightSum = 0;

////                    for (int k = -a; k <= a; k++)
////                    {
////                        int sampleIndex = baseIndex + k;
////                        sampleIndex = sampleIndex < 0 ? 0 : sampleIndex >= inputFrames ? inputFrames - 1 : sampleIndex;

////                        double distance = fractionalPosition - sampleIndex;
////                        float weight = (float)LanczosKernel(distance, a);

////                        result += inputData[sampleIndex * channels + ch] * weight;
////                        weightSum += weight;
////                    }

////                    tempOutput[i * channels + ch] = result / Math.Max(weightSum, 1e-8f);
////                }
////            });

////            // Slice and return final output
////            float[] finalOutput = tempOutput[..(outputFrames * channels)];
////            ArrayPool<float>.Shared.Return(tempOutput, clearArray: true);
////            return finalOutput;
////        }

////        // Lanczos interpolation kernel: sinc(x) * sinc(x / a)
////        private static double LanczosKernel(double x, int a)
////        {
////            x = Math.Abs(x);
////            if (x == 0) return 1;
////            if (x >= a) return 0;

////            double pix = Math.PI * x;
////            return a * Math.Sin(pix) * Math.Sin(pix / a) / (pix * pix);
////        }
////    }
////}
