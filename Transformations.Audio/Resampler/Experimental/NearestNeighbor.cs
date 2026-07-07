using NAudio.Wave;
using System;
using System.Threading.Tasks;

namespace NAudio.Transformations.Resampler.Experimental
{
    /// <summary>
    /// Provides a pure nearest–neighbor resampling algorithm.
    /// This version selects the closest input sample (by rounding) for each output sample,
    /// avoiding the cost of interpolation entirely.
    /// </summary>
    /// <remarks>Not as good at downsampling as Linear.</remarks>
    public static class NearestNeighbor
    {
        public static float[] Resample(float[] inputData, int inRate, int outRate, int channels)
        {
            PureNearestNeighborResampler resampler = new PureNearestNeighborResampler();
            return resampler.Resample(inputData, inRate, outRate, channels);
        }
    }

    /// <summary>
    /// Implements IAudioResampler using pure nearest neighbor resampling. 
    /// A gentle smoothing filter is applied to the result.
    /// </summary>
    public class PureNearestNeighborResampler : IAudioResampler
    {
        public float[] Resample(float[] inputData, int inRate, int outRate, int channels)
        {
            // Validate inputs.
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

            int inputFrames = inputData.Length / channels;
            // Compute output frame count based on the sample rate ratio.
            int outputFrames = (int)Math.Round((double)inputFrames * outRate / inRate);
            float[] output = new float[outputFrames * channels];

            // Compute the ratio between input and output frames.
            double sampleRatio = (double)inputFrames / outputFrames;

            // Use Parallel.For to process each output frame.
            Parallel.For(0, outputFrames, i =>
            {
                // For pure nearest neighbor, round the desired source position.
                int nearest = (int)Math.Round(i * sampleRatio);
                if (nearest < 0)
                    nearest = 0;
                if (nearest >= inputFrames)
                    nearest = inputFrames - 1;
                int outIndex = i * channels;
                int inIndex = nearest * channels;
                for (int ch = 0; ch < channels; ch++)
                {
                    output[outIndex + ch] = inputData[inIndex + ch];
                }
            });

            // --- Apply a gentle smoothing filter ---
            //float smoothingFactor = 0.0001f; // Adjust for more or less smoothing.
            //output = ApplyGentleSmoothing(output, channels, smoothingFactor);
            return output;
        }

        ///// <summary>
        ///// Applies a simple one-pole IIR low-pass filter (per channel) to gently smooth the result.
        ///// </summary>
        //private static float[] ApplyGentleSmoothing(float[] data, int channels, float smoothingFactor)
        //{
        //    int frames = data.Length / channels;
        //    float[] smoothed = new float[data.Length];
        //    // Process each channel independently.
        //    for (int ch = 0; ch < channels; ch++)
        //    {
        //        // Leave the first sample unchanged.
        //        int idx = ch;
        //        smoothed[idx] = data[idx];
        //        for (int i = 1; i < frames; i++)
        //        {
        //            idx = i * channels + ch;
        //            int prevIdx = (i - 1) * channels + ch;
        //            // One-pole lowpass filter: blend current sample with previous filtered sample.
        //            smoothed[idx] = smoothingFactor * data[idx] + (1.0f - smoothingFactor) * smoothed[prevIdx];
        //        }
        //    }
        //    return smoothed;
        //}

        public float[] Resample(float[] inputData, WaveFormat inFormat, WaveFormat outFormat)
        {
            return Resample(inputData, inFormat.SampleRate, outFormat.SampleRate, inFormat.Channels);
        }
    }
}


////using NAudio.Wave;

////namespace NAudio.Transformations.Resampler.Experimental
////{
////    /// <summary>
////    /// Provides a pure nearest–neighbor resampling algorithm.
////    /// This version selects the closest input sample (by rounding) for each output sample,
////    /// avoiding the cost of interpolation entirely.
////    /// </summary>
////    /// <remarks>Not as good at downsampling as Linear.</remarks>
////    public static class NearestNeighbor
////    {
////        public static float[] Resample(float[] inputData, int inRate, int outRate, int channels)
////        {
////            PureNearestNeighborResampler resampler = new PureNearestNeighborResampler();
////            return resampler.Resample(inputData, inRate, outRate, channels);
////        }
////    }

////    public class PureNearestNeighborResampler : IAudioResampler
////    {
////        public float[] Resample(float[] inputData, int inRate, int outRate, int channels)
////        {
////            // Validate inputs.
////            if (inRate <= 0 || outRate <= 0)
////                throw new ArgumentException("Sample rates must be positive.");
////            if (channels <= 0)
////                throw new ArgumentException("Channel count must be positive.");
////            if (inputData == null || inputData.Length == 0)
////                throw new ArgumentException("Input data cannot be null or empty.");
////            if (inputData.Length % channels != 0)
////                throw new ArgumentException("Input data length must be divisible by the number of channels.");
////            if (inRate == outRate)
////                return inputData;

////            int inputFrames = inputData.Length / channels;
////            // Compute output frame count based on the sample rate ratio.
////            int outputFrames = (int)Math.Round((double)inputFrames * outRate / inRate);
////            float[] output = new float[outputFrames * channels];

////            // Compute a ratio between input and output frame counts.
////            double sampleRatio = (double)inputFrames / outputFrames;

////            // Using Parallel.For to spread the loop over available cores.
////            Parallel.For(0, outputFrames, i =>
////            {
////                // For pure nearest neighbor, we simply round the computed index.
////                int nearest = (int)Math.Round(i * sampleRatio);
////                if (nearest < 0)
////                    nearest = 0;
////                if (nearest >= inputFrames)
////                    nearest = inputFrames - 1;
////                int outIndex = i * channels;
////                int inIndex = nearest * channels;
////                for (int ch = 0; ch < channels; ch++)
////                {
////                    output[outIndex + ch] = inputData[inIndex + ch];
////                }
////            });

////            return output;
////        }

////        public float[] Resample(float[] inputData, WaveFormat inFormat, WaveFormat outFormat)
////        {
////            return Resample(inputData, inFormat.SampleRate, outFormat.SampleRate, inFormat.Channels);
////        }
////    }
////}


////////using NAudio.Wave;
////////using System.Buffers;

////////namespace NAudio.Transformations.Resampler.Experimental
////////{
////////    public static class NearestNeighbor
////////    {
////////        public static float[] Resample(float[] inputData, int inRate, int outRate, int channels)
////////        {
////////            NearestNeighborResampler nn = new NearestNeighborResampler();
////////            return nn.Resample(inputData, inRate, outRate, channels);
////////        }
////////    }

////////    public class NearestNeighborResampler : IAudioResampler
////////    {
////////        public float[] Resample(float[] inputData, int inRate, int outRate, int channels)
////////        {
////////            return Resampler(inputData, inRate, outRate, channels);
////////        }

////////        public float[] Resample(float[] inputData, WaveFormat inFormat, WaveFormat outFormat)
////////        {
////////            return Resampler(inputData, inFormat.SampleRate, outFormat.SampleRate, inFormat.Channels);
////////        }

////////        /// <summary>
////////        /// Resamples interleaved audio using an oversampled linear interpolation method.
////////        /// The oversampling factor is determined dynamically from the input signal variance.
////////        /// This version computes the necessary indices and weights on the fly (removing extra array allocations)
////////        /// and uses specialized branches for mono versus multi‑channel audio.
////////        /// </summary>
////////        public static float[] Resampler(float[] inputData, int inRate, int outRate, int channels)
////////        {
////////            // Validate inputs.
////////            if (inRate <= 0 || outRate <= 0)
////////                throw new ArgumentException("Sample rates must be positive.");
////////            if (channels <= 0)
////////                throw new ArgumentException("Channel count must be positive.");
////////            if (inputData == null || inputData.Length == 0)
////////                throw new ArgumentException("Input data cannot be null or empty.");
////////            if (inputData.Length % channels != 0)
////////                throw new ArgumentException("Input data length must be divisible by the number of channels.");
////////            if (inRate == outRate)
////////                return inputData;

////////            // Determine oversampling factor based on input dynamics.
////////            int oversamplingFactor = DetermineOversamplingFactor(inputData, channels);
////////            int oversampledRate = outRate * oversamplingFactor;
////////            // This ratio tells us how many input frames correspond to one oversampled output frame.
////////            double ratio = (double)inRate / oversampledRate;

////////            int inputFrames = inputData.Length / channels;
////////            int oversampledFrames = (int)(inputFrames / ratio);

////////            // Rent a temporary buffer for the oversampled output.
////////            float[] oversampledOutput = ArrayPool<float>.Shared.Rent(oversampledFrames * channels);

////////            // Compute interpolation on the fly.
////////            if (channels == 1)
////////            {
////////                // For mono, avoid inner loops over channels.
////////                Parallel.For(0, oversampledFrames, i =>
////////                {
////////                    double pos = i * ratio;
////////                    int nearest = (int)pos; // For positive values, a cast to int yields floor(pos)
////////                    double weight = pos - nearest;
////////                    double invWeight = 1.0 - weight;
////////                    int next = (nearest < inputFrames - 1) ? nearest + 1 : nearest;

////////                    oversampledOutput[i] = (float)(invWeight * inputData[nearest] + weight * inputData[next]);
////////                });
////////            }
////////            else
////////            {
////////                // For multi‑channel, process in chunks to reduce overhead.
////////                int chunkSize = Math.Max(1024, oversampledFrames / (Environment.ProcessorCount * 4));
////////                Parallel.For(0, (oversampledFrames + chunkSize - 1) / chunkSize, chunk =>
////////                {
////////                    int start = chunk * chunkSize;
////////                    int end = Math.Min(oversampledFrames, start + chunkSize);
////////                    for (int i = start; i < end; i++)
////////                    {
////////                        double pos = i * ratio;
////////                        int nearest = (int)pos;
////////                        double weight = pos - nearest;
////////                        double invWeight = 1.0 - weight;
////////                        int next = (nearest < inputFrames - 1) ? nearest + 1 : nearest;
////////                        int outBase = i * channels;
////////                        int baseNearest = nearest * channels;
////////                        int baseNext = next * channels;
////////                        for (int ch = 0; ch < channels; ch++)
////////                        {
////////                            oversampledOutput[outBase + ch] =
////////                                (float)(invWeight * inputData[baseNearest + ch] + weight * inputData[baseNext + ch]);
////////                        }
////////                    }
////////                });
////////            }

////////            float[] finalOutput;
////////            if (oversamplingFactor == 1)
////////            {
////////                // No downsampling stage is necessary.
////////                finalOutput = new float[oversampledFrames * channels];
////////                Array.Copy(oversampledOutput, finalOutput, finalOutput.Length);
////////            }
////////            else
////////            {
////////                // Downsample (by averaging groups of frames) back to the target rate.
////////                finalOutput = Downsample(oversampledOutput, oversampledFrames, channels, oversamplingFactor);
////////            }

////////            ArrayPool<float>.Shared.Return(oversampledOutput, true);
////////            return finalOutput;
////////        }

////////        /// <summary>
////////        /// Determines an oversampling factor based on the variance between successive frames.
////////        /// Lower variance suggests a smooth signal (no oversampling needed), while higher variance requires more.
////////        /// </summary>
////////        private static int DetermineOversamplingFactor(float[] inputData, int channels)
////////        {
////////            int frames = inputData.Length / channels;
////////            double variance = 0.0;

////////            // Compute the variance of the differences between successive frames.
////////            for (int i = 0; i < frames - 1; i++)
////////            {
////////                for (int ch = 0; ch < channels; ch++)
////////                {
////////                    double diff = inputData[(i + 1) * channels + ch] - inputData[i * channels + ch];
////////                    variance += diff * diff;
////////                }
////////            }
////////            variance /= (frames - 1);

////////            if (variance < 0.01)
////////                return 1;  // Very smooth: no oversampling
////////            else if (variance < 0.1)
////////                return 2;  // Moderate oversampling
////////            else
////////                return 4;  // High oversampling for complex signals
////////        }

////////        /// <summary>
////////        /// Downsamples the oversampled output by averaging groups of frames.
////////        /// Specialized unrolling is provided for oversampling factors commonly used (2 and 4).
////////        /// </summary>
////////        private static float[] Downsample(float[] oversampledOutput, int oversampledFrames, int channels, int oversamplingFactor)
////////        {
////////            int downsampledFrames = oversampledFrames / oversamplingFactor;
////////            float[] downsampledOutput = new float[downsampledFrames * channels];

////////            if (channels == 1)
////////            {
////////                Parallel.For(0, downsampledFrames, i =>
////////                {
////////                    int baseIndex = i * oversamplingFactor;
////////                    float sum = 0.0f;
////////                    if (oversamplingFactor == 2)
////////                    {
////////                        sum = oversampledOutput[baseIndex] + oversampledOutput[baseIndex + 1];
////////                        downsampledOutput[i] = sum * 0.5f;
////////                    }
////////                    else if (oversamplingFactor == 4)
////////                    {
////////                        sum = oversampledOutput[baseIndex] +
////////                              oversampledOutput[baseIndex + 1] +
////////                              oversampledOutput[baseIndex + 2] +
////////                              oversampledOutput[baseIndex + 3];
////////                        downsampledOutput[i] = sum * 0.25f;
////////                    }
////////                    else
////////                    {
////////                        for (int j = 0; j < oversamplingFactor; j++)
////////                            sum += oversampledOutput[baseIndex + j];
////////                        downsampledOutput[i] = sum / oversamplingFactor;
////////                    }
////////                });
////////            }
////////            else
////////            {
////////                Parallel.For(0, downsampledFrames, i =>
////////                {
////////                    int outBase = i * channels;
////////                    int baseIndex = i * oversamplingFactor;
////////                    for (int ch = 0; ch < channels; ch++)
////////                    {
////////                        float sum = 0.0f;
////////                        if (oversamplingFactor == 2)
////////                        {
////////                            int index0 = baseIndex * channels + ch;
////////                            int index1 = (baseIndex + 1) * channels + ch;
////////                            sum = oversampledOutput[index0] + oversampledOutput[index1];
////////                            downsampledOutput[outBase + ch] = sum * 0.5f;
////////                        }
////////                        else if (oversamplingFactor == 4)
////////                        {
////////                            int index0 = baseIndex * channels + ch;
////////                            int index1 = (baseIndex + 1) * channels + ch;
////////                            int index2 = (baseIndex + 2) * channels + ch;
////////                            int index3 = (baseIndex + 3) * channels + ch;
////////                            sum = oversampledOutput[index0] + oversampledOutput[index1] +
////////                                  oversampledOutput[index2] + oversampledOutput[index3];
////////                            downsampledOutput[outBase + ch] = sum * 0.25f;
////////                        }
////////                        else
////////                        {
////////                            for (int j = 0; j < oversamplingFactor; j++)
////////                                sum += oversampledOutput[((baseIndex + j) * channels) + ch];
////////                            downsampledOutput[outBase + ch] = sum / (float)oversamplingFactor;
////////                        }
////////                    }
////////                });
////////            }

////////            return downsampledOutput;
////////        }
////////    }
////////}





//////using NAudio.Wave;
//////using System;
//////using System.Buffers;
//////using System.Threading.Channels;
//////using System.Threading.Tasks;

//////namespace NAudio.Transformations.Resampler.Experimental
//////{
//////    public static class NearestNeighbor
//////    {
//////        public static float[] Resample(float[] inputData, int inRate, int outRate, int channels)
//////        {
//////            NearestNeighborResampler lr = new NearestNeighborResampler();
//////            return lr.Resample(inputData, inRate, outRate, channels);
//////        }
//////    }

//////    public class NearestNeighborResampler : IAudioResampler
//////    {
//////        public float[] Resample(float[] inputData, int inRate, int outRate, int channels)
//////        {
//////            //LinearResampler lr = new LinearResampler();
//////            return Resampler(inputData, inRate, outRate, channels);
//////        }

//////        public float[] Resample(float[] inputData, WaveFormat inFormat, WaveFormat outFormat)
//////        {
//////            return Resampler(inputData, inFormat.SampleRate, outFormat.SampleRate, inFormat.Channels);
//////        }

//////        /// <summary>
//////        /// Resamples interleaved audio using an oversampled linear interpolation method.
//////        /// The oversampling factor is determined dynamically from the input signal variance.
//////        /// </summary>
//////        public static float[] Resampler(float[] inputData, int inRate, int outRate, int channels)
//////        {
//////            if (inRate <= 0 || outRate <= 0)
//////                throw new ArgumentException("Sample rates must be positive.");
//////            if (channels <= 0)
//////                throw new ArgumentException("Channel count must be positive.");
//////            if (inputData == null || inputData.Length == 0)
//////                throw new ArgumentException("Input data cannot be null or empty.");
//////            if (inputData.Length % channels != 0)
//////                throw new ArgumentException("Input data length must be divisible by the number of channels.");
//////            if (inRate == outRate)
//////                return inputData;

//////            // Determine oversampling factor based on signal complexity.
//////            int oversamplingFactor = DetermineOversamplingFactor(inputData, channels);
//////            // Compute oversampled target rate.
//////            int oversampledRate = outRate * oversamplingFactor;
//////            double ratio = (double)inRate / oversampledRate;

//////            int inputFrames = inputData.Length / channels;
//////            int oversampledFrames = (int)(inputFrames / ratio);

//////            // Rent a temporary buffer for oversampled output.
//////            float[] oversampledOutput = ArrayPool<float>.Shared.Rent(oversampledFrames * channels);

//////            // Precompute interpolation indices and weights for each oversampled frame.
//////            int[] nearestIndices = new int[oversampledFrames];
//////            int[] nextIndices = new int[oversampledFrames];
//////            double[] weights = new double[oversampledFrames];

//////            for (int i = 0; i < oversampledFrames; i++)
//////            {
//////                double position = i * ratio;
//////                int nearest = (int)Math.Floor(position);
//////                nearestIndices[i] = nearest;
//////                // Clamp the next index to ensure it is within bounds.
//////                nextIndices[i] = (nearest < inputFrames - 1) ? nearest + 1 : nearest;
//////                weights[i] = position - nearest;
//////            }

//////            // Process weighted interpolation in parallel
//////            int chunkSize = Math.Max(1024, oversampledFrames / (Environment.ProcessorCount * 4));
//////            Parallel.For(0, (oversampledFrames + chunkSize - 1) / chunkSize, chunk =>
//////            {
//////                int start = chunk * chunkSize;
//////                int end = Math.Min(oversampledFrames, start + chunkSize);
//////                for (int i = start; i < end; i++)
//////                {
//////                    int nIndex = nearestIndices[i];
//////                    int nextIndex = nextIndices[i];
//////                    double weight = weights[i];

//////                    int outBase = i * channels;
//////                    int inputBaseNearest = nIndex * channels;
//////                    int inputBaseNext = nextIndex * channels;
//////                    for (int ch = 0; ch < channels; ch++)
//////                    {
//////                        oversampledOutput[outBase + ch] =
//////                            (float)(((1 - weight) * inputData[inputBaseNearest + ch]) +
//////                                    (weight * inputData[inputBaseNext + ch]));
//////                    }
//////                }
//////            });

//////            float[] finalOutput;
//////            // If no oversampling is needed, simply copy the result.
//////            if (oversamplingFactor == 1)
//////            {
//////                finalOutput = new float[oversampledFrames * channels];
//////                Array.Copy(oversampledOutput, finalOutput, finalOutput.Length);
//////            }
//////            else
//////            {
//////                // Downsample (average groups of oversampled frames) back to target rate.
//////                finalOutput = Downsample(oversampledOutput, oversampledFrames, channels, oversamplingFactor);
//////            }

//////            ArrayPool<float>.Shared.Return(oversampledOutput, true);
//////            return finalOutput;
//////        }

//////        /// <summary>
//////        /// Determines an oversampling factor based on the variance of adjacent frame differences.
//////        /// Iterating over frames (not samples) gives a better idea of complexity.
//////        /// </summary>
//////        private static int DetermineOversamplingFactor(float[] inputData, int channels)
//////        {
//////            int frames = inputData.Length / channels;
//////            double variance = 0.0;

//////            // Compute variance of differences between successive frames.
//////            for (int i = 0; i < frames - 1; i++)
//////            {
//////                for (int ch = 0; ch < channels; ch++)
//////                {
//////                    double diff = inputData[(i + 1) * channels + ch] - inputData[i * channels + ch];
//////                    variance += diff * diff;
//////                }
//////            }
//////            variance /= (frames - 1);

//////            if (variance < 0.01)
//////                return 1;  // Low variance: no oversampling needed
//////            else if (variance < 0.1)
//////                return 2;  // Moderate oversampling
//////            else
//////                return 4;  // High oversampling needed for complex signals
//////        }

//////        /// <summary>
//////        /// Downsamples the oversampled output by averaging groups of frames.
//////        /// Implements special-case unrolling for oversampling factors of 2 and 4.
//////        /// </summary>
//////        private static float[] Downsample(float[] oversampledOutput, int oversampledFrames, int channels, int oversamplingFactor)
//////        {
//////            int downsampledFrames = oversampledFrames / oversamplingFactor;
//////            float[] downsampledOutput = new float[downsampledFrames * channels];

//////            Parallel.For(0, downsampledFrames, i =>
//////            {
//////                int outBase = i * channels;
//////                int baseIndex = i * oversamplingFactor;
//////                for (int ch = 0; ch < channels; ch++)
//////                {
//////                    float sum = 0.0f;
//////                    if (oversamplingFactor == 2)
//////                    {
//////                        int index0 = (baseIndex * channels) + ch;
//////                        int index1 = ((baseIndex + 1) * channels) + ch;
//////                        sum = oversampledOutput[index0] + oversampledOutput[index1];
//////                        downsampledOutput[outBase + ch] = sum * 0.5f;
//////                    }
//////                    else if (oversamplingFactor == 4)
//////                    {
//////                        int index0 = (baseIndex * channels) + ch;
//////                        int index1 = ((baseIndex + 1) * channels) + ch;
//////                        int index2 = ((baseIndex + 2) * channels) + ch;
//////                        int index3 = ((baseIndex + 3) * channels) + ch;
//////                        sum = oversampledOutput[index0] + oversampledOutput[index1] +
//////                              oversampledOutput[index2] + oversampledOutput[index3];
//////                        downsampledOutput[outBase + ch] = sum * 0.25f;
//////                    }
//////                    else
//////                    {
//////                        for (int j = 0; j < oversamplingFactor; j++)
//////                        {
//////                            sum += oversampledOutput[((baseIndex + j) * channels) + ch];
//////                        }
//////                        downsampledOutput[outBase + ch] = sum / oversamplingFactor;
//////                    }
//////                }
//////            });

//////            return downsampledOutput;
//////        }
//////    }
//////}
