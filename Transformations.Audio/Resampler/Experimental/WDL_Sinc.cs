using System;
using System.Threading.Tasks;

namespace Transformations.Audio.Resampler.Experimental
{
    public static class WDL_Sinc
    {
        private const int FilterHalf = 8;
        private const int TableResolution = 1024; // Controls accuracy vs. table size.
        // The table is precomputed one time. Its size is (2*FilterHalf*TableResolution)+1.
        private static readonly double[] SincTable = PrecomputeSincTable(FilterHalf, TableResolution);
        private const int TableSize = (2 * FilterHalf * TableResolution) + 1;
        private const int MaxTableIndex = 2 * FilterHalf * TableResolution;

        public static float[] Resample(float[] inputData, int inRate, int outRate, int channels)
        {
            // Input validation.
            if (inRate <= 0 || outRate <= 0)
                throw new ArgumentException("Sample rates must be positive.");
            if (channels <= 0)
                throw new ArgumentException("Channel count must be positive.");
            if (inputData == null || inputData.Length == 0)
                throw new ArgumentException("Input data cannot be null or empty.");
            if (inputData.Length % channels != 0)
                throw new ArgumentException("Input data length must be divisible by the number of channels.");
            if (inRate == outRate)
                return inputData;

            double ratio = (double)inRate / outRate;
            int inputFrames = inputData.Length / channels;
            int outputFrames = (int)(inputFrames / ratio);
            float[] output = new float[outputFrames * channels];

            // Process output frames in parallel.
            Parallel.For(0, outputFrames, i =>
            {
                double srcPos = i * ratio;
                int baseIndex = (int)srcPos;      // This computes the floor (truncation is equivalent for positive values).
                double fracPos = srcPos - baseIndex;
                for (int ch = 0; ch < channels; ch++)
                {
                    double sum = 0.0;
                    double norm = 0.0;
                    // Loop over filter taps.
                    for (int j = -FilterHalf; j <= FilterHalf; j++)
                    {
                        int pos = baseIndex + j;
                        // Clamp the position between 0 and inputFrames-1.
                        int clampedPos = (pos < 0) ? 0 : (pos >= inputFrames ? inputFrames - 1 : pos);
                        // Compute distance from the ideal fractional location.
                        double distance = j - fracPos;
                        // Inlined lookup: compute table index and check bounds.
                        double weight;
                        {
                            // Map distance so that x=-FilterHalf corresponds to index 0
                            double temp = (distance + FilterHalf) * TableResolution;
                            int index = (int)Math.Round(temp);
                            weight = (index < 0 || index > MaxTableIndex) ? 0.0 : SincTable[index];
                        }
                        sum += weight * inputData[clampedPos * channels + ch];
                        norm += weight;
                    }
                    // Normalize if needed.
                    if (norm != 0)
                        sum /= norm;
                    output[i * channels + ch] = (float)sum;
                }
            });

            return output;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static double[] PrecomputeSincTable(int half, int resolution)
        {
            int size = (2 * half * resolution) + 1;
            double[] table = new double[size];
            int center = size / 2;
            double invResolution = 1.0 / resolution;
            for (int i = 0; i < size; i++)
            {
                double x = (i - center) * invResolution;
                double sinc = (x == 0.0) ? 1.0 : Math.Sin(Math.PI * x) / (Math.PI * x);
                // Use a Hann window.
                double window = 0.5 * (1.0 + Math.Cos(Math.PI * x / half));
                table[i] = sinc * window;
            }
            return table;
        }
    }
}

