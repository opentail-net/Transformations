using NAudio.Wave;
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace NAudio.Transformations.Resampler.Experimental
{
    /// <summary>
    /// A cubic interpolation resampler (Mitchell-Netravali/Catmull–Rom style).
    /// </summary>
    /// <remarks>The experimental Hermite and Cubic variants don’t really give any noticeable quality improvement over linear, yet they are slower.</remarks>
    public static class Cubic
    {
        internal static float[] Resample(float[] inputData, int inRate, int outRate, int channels)
        {
            CubicResampler cr = new CubicResampler();
            return cr.Resample(inputData, inRate, outRate, channels);
        }
    }

    public class CubicResampler : IAudioResampler
    {
        // Precompute a SIMD vector for indices [0, 1, 2, ..., Vector<float>.Count - 1]
        private readonly Vector<float> SIMDIndexVector;

        public CubicResampler()
        {
            int simdWidth = Vector<float>.Count;
            float[] indices = new float[simdWidth];
            for (int i = 0; i < simdWidth; i++)
            {
                indices[i] = i;
            }
            SIMDIndexVector = new Vector<float>(indices);
        }


        public float[] Resample(float[] inputData, WaveFormat inFormat, WaveFormat outFormat)
        {
            return Resample(inputData, inFormat.SampleRate, outFormat.SampleRate, inFormat.Channels);
        }

        /// <summary>
        /// Resamples interleaved float audio using cubic interpolation.
        /// </summary>
        public float[] Resample(float[] inputData, int inRate, int outRate, int channels)
        {
            if (inRate <= 0 || outRate <= 0)
                throw new ArgumentException("Sample rates must be positive.");
            if (channels <= 0)
                throw new ArgumentException("Channel count must be positive.");
            if (inputData == null || inputData.Length == 0)
                throw new ArgumentException("Input data cannot be null or empty.");
            if (inputData.Length % channels != 0)
                throw new ArgumentException("Input data length must be divisible by the number of channels.");

            float ratio = (float)inRate / outRate;
            int inputFrames = inputData.Length / channels;
            int outputFrames = (int)(inputFrames / ratio);
            if (outputFrames == 0)
                return Array.Empty<float>();

            // Rent a temporary buffer for performance.
            float[] outputBuffer = ArrayPool<float>.Shared.Rent(outputFrames * channels);

            // Use SIMD if we're processing mono data and hardware acceleration is available.
            if (channels == 1 && Vector.IsHardwareAccelerated)
            {
                ResampleMonoSIMD(inputData, outputBuffer, inputFrames, outputFrames, ratio);
            }
            else
            {
                ResampleStandard(inputData, outputBuffer, inputFrames, outputFrames, ratio, channels);
            }

            // Copy to the exact-size output array.
            float[] finalOutput = new float[outputFrames * channels];
            Array.Copy(outputBuffer, finalOutput, finalOutput.Length);
            ArrayPool<float>.Shared.Return(outputBuffer, clearArray: true);

            return finalOutput;
        }

        // Make this an instance method (removed 'static') so that it can use SIMDIndexVector.
        private void ResampleMonoSIMD(float[] input, float[] output, int inputFrames, int outputFrames, float ratio)
        {
            int simdWidth = Vector<float>.Count;
            int simdBlockCount = outputFrames / simdWidth;

            Parallel.For(0, simdBlockCount, block =>
            {
                int blockStart = block * simdWidth;
                // Compute base indices: [blockStart, blockStart+1, ...] using the precomputed SIMD vector.
                Vector<float> baseIndicesVector = new Vector<float>(blockStart) + SIMDIndexVector;
                Vector<float> fractional = new Vector<float>(ratio) * baseIndicesVector;
                Vector<int> intPart = Vector.ConvertToInt32(fractional);
                Vector<float> t = fractional - Vector.ConvertToSingle(intPart);

                for (int lane = 0; lane < simdWidth; lane++)
                {
                    int idx = intPart[lane];
                    float frac = t[lane];

                    float y0 = GetSafe(input, inputFrames, idx - 1);
                    float y1 = GetSafe(input, inputFrames, idx);
                    float y2 = GetSafe(input, inputFrames, idx + 1);
                    float y3 = GetSafe(input, inputFrames, idx + 2);

                    output[blockStart + lane] = CubicInterpolate(y0, y1, y2, y3, frac);
                }
            });

            // Process any remaining samples that didn't fit in the SIMD blocks.
            for (int i = simdBlockCount * simdWidth; i < outputFrames; i++)
            {
                float fractional = i * ratio;
                int baseIndex = (int)fractional;
                float t = fractional - baseIndex;

                float y0 = GetSafe(input, inputFrames, baseIndex - 1);
                float y1 = GetSafe(input, inputFrames, baseIndex);
                float y2 = GetSafe(input, inputFrames, baseIndex + 1);
                float y3 = GetSafe(input, inputFrames, baseIndex + 2);

                output[i] = CubicInterpolate(y0, y1, y2, y3, t);
            }
        }

        // This method can remain static since it doesn't use any instance members.
        private static void ResampleStandard(float[] input, float[] output, int inputFrames, int outputFrames, float ratio, int channels)
        {
            Parallel.For(0, outputFrames, i =>
            {
                float fractional = i * ratio;
                int baseIndex = (int)fractional;
                float t = fractional - baseIndex;

                for (int ch = 0; ch < channels; ch++)
                {
                    float y0 = GetSafe(input, inputFrames, baseIndex - 1, channels, ch);
                    float y1 = GetSafe(input, inputFrames, baseIndex, channels, ch);
                    float y2 = GetSafe(input, inputFrames, baseIndex + 1, channels, ch);
                    float y3 = GetSafe(input, inputFrames, baseIndex + 2, channels, ch);

                    output[i * channels + ch] = CubicInterpolate(y0, y1, y2, y3, t);
                }
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float GetSafe(float[] data, int frames, int frame, int channels = 1, int channel = 0)
        {
            frame = Math.Clamp(frame, 0, frames - 1);
            return data[frame * channels + channel];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float CubicInterpolate(float y0, float y1, float y2, float y3, float t)
        {
            // Standard Catmull–Rom cubic interpolation formula.
            float a = -0.5f * y0 + 1.5f * y1 - 1.5f * y2 + 0.5f * y3;
            float b = y0 - 2.5f * y1 + 2f * y2 - 0.5f * y3;
            float c = -0.5f * y0 + 0.5f * y2;
            float d = y1;
            return ((a * t + b) * t + c) * t + d;
        }
    }
}


//using NAudio.Wave;
//using System;
//using System.Buffers;
//using System.Numerics;
//using System.Runtime.CompilerServices;
//using System.Threading.Tasks;

//namespace NAudio.Transformations.Resampler.Experimental
//{
//    /// <summary>
//    /// A cubic interpolation resampler (Mitchell-Netravali/Catmull–Rom style).
//    /// Faster than spline and higher quality than linear, but not as precise as sinc.
//    /// </summary>
//    public static class Cubic
//    {
//        internal static float[] Resample(float[] inputData, int inRate, int outRate, int channels)
//        {
//            CubicResampler cr = new CubicResampler();
//            return cr.Resample(inputData, inRate, outRate, channels);
//            //throw new NotImplementedException();
//        }
//    }

//    public  class CubicResampler : IAudioResampler
//    {
//        /// <summary>
//        /// Resamples interleaved audio data using linear interpolation, based on input and output <see cref="WaveFormat"/>s.
//        /// </summary>
//        /// <param name="inputData">Interleaved audio samples as a float array.</param>
//        /// <param name="inFormat">The <see cref="WaveFormat"/> describing the input sample rate and channel count.</param>
//        /// <param name="outFormat">The <see cref="WaveFormat"/> describing the desired output sample rate and channel count.</param>
//        /// <returns>Resampled interleaved audio data as a float array.</returns>
//        /// <exception cref="ArgumentNullException">Thrown if either <paramref name="inFormat"/> or <paramref name="outFormat"/> is null.</exception>
//        public float[] Resample(float[] inputData, WaveFormat inFormat, WaveFormat outFormat)
//        {
//            if (inFormat == null || outFormat == null)
//                throw new ArgumentNullException("Both input and output WaveFormat must be provided.");

//            return Resample(inputData, inFormat.SampleRate, outFormat.SampleRate, inFormat.Channels);
//        }

//        // Precompute a SIMD vector for indices [0, 1, 2, ..., Vector<float>.Count - 1]
//        private readonly Vector<float> SIMDIndexVector;

//        public  CubicResampler()
//        {
//            int simdWidth = Vector<float>.Count;
//            float[] indices = new float[simdWidth];
//            for (int i = 0; i < simdWidth; i++)
//            {
//                indices[i] = i;
//            }
//            SIMDIndexVector = new Vector<float>(indices);
//        }

//        /// <summary>
//        /// Resamples interleaved float audio using cubic interpolation.
//        /// </summary>
//        public float[] Resample(float[] inputData, int inRate, int outRate, int channels)
//        {
//            if (inRate <= 0 || outRate <= 0)
//                throw new ArgumentException("Sample rates must be positive.");
//            if (channels <= 0)
//                throw new ArgumentException("Channel count must be positive.");
//            if (inputData == null || inputData.Length == 0)
//                throw new ArgumentException("Input data cannot be null or empty.");
//            if (inputData.Length % channels != 0)
//                throw new ArgumentException("Input data length must be divisible by the number of channels.");

//            float ratio = (float)inRate / outRate;
//            int inputFrames = inputData.Length / channels;
//            int outputFrames = (int)(inputFrames / ratio);
//            if (outputFrames == 0)
//                return Array.Empty<float>();

//            // Rent a temporary buffer for performance.
//            float[] outputBuffer = ArrayPool<float>.Shared.Rent(outputFrames * channels);

//            // Use SIMD if we're processing mono data and hardware acceleration is available.
//            if (channels == 1 && Vector.IsHardwareAccelerated)
//            {
//                ResampleMonoSIMD(inputData, outputBuffer, inputFrames, outputFrames, ratio);
//            }
//            else
//            {
//                ResampleStandard(inputData, outputBuffer, inputFrames, outputFrames, ratio, channels);
//            }

//            // Copy to the exact-size output array.
//            float[] finalOutput = new float[outputFrames * channels];
//            Array.Copy(outputBuffer, finalOutput, finalOutput.Length);
//            ArrayPool<float>.Shared.Return(outputBuffer, clearArray: true);

//            return finalOutput;
//        }

//        private static void ResampleStandard(float[] input, float[] output, int inputFrames, int outputFrames, float ratio, int channels)
//        {
//            Parallel.For(0, outputFrames, i =>
//            {
//                float fractional = i * ratio;
//                int baseIndex = (int)fractional;
//                float t = fractional - baseIndex;

//                for (int ch = 0; ch < channels; ch++)
//                {
//                    float y0 = GetSafe(input, inputFrames, baseIndex - 1, channels, ch);
//                    float y1 = GetSafe(input, inputFrames, baseIndex, channels, ch);
//                    float y2 = GetSafe(input, inputFrames, baseIndex + 1, channels, ch);
//                    float y3 = GetSafe(input, inputFrames, baseIndex + 2, channels, ch);

//                    output[i * channels + ch] = CubicInterpolate(y0, y1, y2, y3, t);
//                }
//            });
//        }

//        private static void ResampleMonoSIMD(float[] input, float[] output, int inputFrames, int outputFrames, float ratio)
//        {
//            int simdWidth = Vector<float>.Count;
//            int simdBlockCount = outputFrames / simdWidth;

//            Parallel.For(0, simdBlockCount, block =>
//            {
//                int blockStart = block * simdWidth;
//                // Compute base indices: [blockStart, blockStart+1, ...] using the precomputed SIMD vector.
//                Vector<float> baseIndicesVector = new Vector<float>(blockStart) + SIMDIndexVector;
//                Vector<float> fractional = new Vector<float>(ratio) * baseIndicesVector;
//                Vector<int> intPart = Vector.ConvertToInt32(fractional);
//                Vector<float> t = fractional - Vector.ConvertToSingle(intPart);

//                for (int lane = 0; lane < simdWidth; lane++)
//                {
//                    int idx = intPart[lane];
//                    float frac = t[lane];

//                    float y0 = GetSafe(input, inputFrames, idx - 1);
//                    float y1 = GetSafe(input, inputFrames, idx);
//                    float y2 = GetSafe(input, inputFrames, idx + 1);
//                    float y3 = GetSafe(input, inputFrames, idx + 2);

//                    output[blockStart + lane] = CubicInterpolate(y0, y1, y2, y3, frac);
//                }
//            });

//            // Fallback: Process any remaining samples that didn't fit into a SIMD block.
//            for (int i = simdBlockCount * simdWidth; i < outputFrames; i++)
//            {
//                float fractional = i * ratio;
//                int baseIndex = (int)fractional;
//                float t = fractional - baseIndex;

//                float y0 = GetSafe(input, inputFrames, baseIndex - 1);
//                float y1 = GetSafe(input, inputFrames, baseIndex);
//                float y2 = GetSafe(input, inputFrames, baseIndex + 1);
//                float y3 = GetSafe(input, inputFrames, baseIndex + 2);
//                output[i] = CubicInterpolate(y0, y1, y2, y3, t);
//            }
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private static float GetSafe(float[] data, int frames, int frame, int channels = 1, int channel = 0)
//        {
//            frame = Math.Clamp(frame, 0, frames - 1);
//            return data[frame * channels + channel];
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private static float CubicInterpolate(float y0, float y1, float y2, float y3, float t)
//        {
//            // Standard Catmull–Rom cubic interpolation formula.
//            float a = -0.5f * y0 + 1.5f * y1 - 1.5f * y2 + 0.5f * y3;
//            float b = y0 - 2.5f * y1 + 2f * y2 - 0.5f * y3;
//            float c = -0.5f * y0 + 0.5f * y2;
//            float d = y1;
//            return ((a * t + b) * t + c) * t + d;
//        }
//    }
//}



//////using System;
//////using System.Buffers;
//////using System.Linq;
//////using System.Numerics;
//////using System.Threading.Tasks;

//////namespace NAudio.Transformations.Resampler.Experimental
//////{
//////    /// <summary>
//////    /// A cubic interpolation resampler (Mitchell-Netravali style).
//////    /// Faster than spline and higher quality than linear, but not as precise as sinc.
//////    /// </summary>
//////    public static class Cubic
//////    {
//////        /// <summary>
//////        /// Resamples interleaved float audio using cubic interpolation.
//////        /// </summary>
//////        public static float[] Resample(float[] inputData, int inRate, int outRate, int channels)
//////        {
//////            if (inRate <= 0 || outRate <= 0)
//////                throw new ArgumentException("Sample rates must be positive.");
//////            if (channels <= 0)
//////                throw new ArgumentException("Channel count must be positive.");
//////            if (inputData == null || inputData.Length == 0)
//////                throw new ArgumentException("Input data cannot be null or empty.");
//////            if (inputData.Length % channels != 0)
//////                throw new ArgumentException("Input data length must be divisible by the number of channels.");

//////            float ratio = (float)inRate / outRate;
//////            int inputFrames = inputData.Length / channels;
//////            int outputFrames = (int)(inputFrames / ratio);

//////            if (outputFrames == 0)
//////                return Array.Empty<float>();

//////            float[] outputBuffer = ArrayPool<float>.Shared.Rent(outputFrames * channels);

//////            if (channels == 1 && Vector.IsHardwareAccelerated)
//////            {
//////                ResampleMonoSIMD(inputData, outputBuffer, inputFrames, outputFrames, ratio);
//////            }
//////            else
//////            {
//////                ResampleStandard(inputData, outputBuffer, inputFrames, outputFrames, ratio, channels);
//////            }

//////            float[] finalOutput = new float[outputFrames * channels];
//////            Array.Copy(outputBuffer, finalOutput, finalOutput.Length);
//////            ArrayPool<float>.Shared.Return(outputBuffer, clearArray: true);

//////            return finalOutput;
//////        }

//////        private static void ResampleStandard(float[] input, float[] output, int inputFrames, int outputFrames, float ratio, int channels)
//////        {
//////            Parallel.For(0, outputFrames, i =>
//////            {
//////                float fractional = i * ratio;
//////                int baseIndex = (int)fractional;
//////                float t = fractional - baseIndex;

//////                for (int ch = 0; ch < channels; ch++)
//////                {
//////                    float y0 = GetSafe(input, inputFrames, baseIndex - 1, channels, ch);
//////                    float y1 = GetSafe(input, inputFrames, baseIndex, channels, ch);
//////                    float y2 = GetSafe(input, inputFrames, baseIndex + 1, channels, ch);
//////                    float y3 = GetSafe(input, inputFrames, baseIndex + 2, channels, ch);

//////                    float value = CubicInterpolate(y0, y1, y2, y3, t);
//////                    output[i * channels + ch] = value;
//////                }
//////            });
//////        }

//////        private static void ResampleMonoSIMD(float[] input, float[] output, int inputFrames, int outputFrames, float ratio)
//////        {
//////            int simdWidth = Vector<float>.Count;

//////            Parallel.For(0, outputFrames / simdWidth, simdBlock =>
//////            {
//////                int i = simdBlock * simdWidth;

//////                var indexArray = Enumerable.Range(i, simdWidth).Select(x => (float)x).ToArray();
//////                var fractional = new Vector<float>(ratio) * new Vector<float>(indexArray);
//////                var baseIndices = Vector.ConvertToInt32(fractional);
//////                var t = fractional - Vector.ConvertToSingle(baseIndices);

//////                for (int j = 0; j < simdWidth; j++)
//////                {
//////                    int idx = baseIndices[j];
//////                    float frac = t[j];

//////                    float y0 = GetSafe(input, inputFrames, idx - 1);
//////                    float y1 = GetSafe(input, inputFrames, idx);
//////                    float y2 = GetSafe(input, inputFrames, idx + 1);
//////                    float y3 = GetSafe(input, inputFrames, idx + 2);

//////                    float value = CubicInterpolate(y0, y1, y2, y3, frac);
//////                    output[i + j] = value;
//////                }
//////            });

//////            // Fallback for any remaining samples outside SIMD block
//////            for (int i = (outputFrames / simdWidth) * simdWidth; i < outputFrames; i++)
//////            {
//////                float fractional = i * ratio;
//////                int baseIndex = (int)fractional;
//////                float t = fractional - baseIndex;

//////                float y0 = GetSafe(input, inputFrames, baseIndex - 1);
//////                float y1 = GetSafe(input, inputFrames, baseIndex);
//////                float y2 = GetSafe(input, inputFrames, baseIndex + 1);
//////                float y3 = GetSafe(input, inputFrames, baseIndex + 2);

//////                float value = CubicInterpolate(y0, y1, y2, y3, t);
//////                output[i] = value;
//////            }
//////        }

//////        private static float GetSafe(float[] data, int frames, int frame, int channels = 1, int channel = 0)
//////        {
//////            frame = Math.Clamp(frame, 0, frames - 1);
//////            return data[frame * channels + channel];
//////        }

//////        //private static float CubicInterpolate(float y0, float y1, float y2, float y3, float t)
//////        //{
//////        //    float a = -0.5f * y0 + 1.5f * y1 - 1.5f * y2 + 0.5f * y3;
//////        //    float b = y0 - 2.5f * y1 + 2f * y2 - 0.5f * y3;
//////        //    float c = -0.5f * y0 + 0.5f * y2;
//////        //    float d = y1;

//////        //    return ((a * t + b) * t + c) * t + d;
//////        //}
//////        public static float CubicInterpolate(float y0, float y1, float y2, float y3, float t, float b = 0f, float c = 0.5f)
//////        {
//////            float a0 = -b - 6 * c;
//////            float a1 = 6 * b + 30 * c;
//////            float a2 = -12 * b - 48 * c;
//////            float a3 = 8 * b + 24 * c;

//////            float p = ((a0 * t + a1) * t + a2) * t + a3;
//////            // Apply this to normalized input points y0..y3, or adjust as needed
//////            return ((-y0 + 3 * y1 - 3 * y2 + y3) * 0.5f * t * t * t) +
//////                   ((2 * y0 - 5 * y1 + 4 * y2 - y3) * 0.5f * t * t) +
//////                   ((-y0 + y2) * 0.5f * t) + y1;
//////        }

//////    }
//////}
