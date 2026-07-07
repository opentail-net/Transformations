using System;
using System.Threading.Tasks;
using NAudio.Wave;

namespace Transformations.Audio.Resampler
{
    /// <summary>
    /// </summary>
    /// <remarks>
    /// v2: C2-continuous (curvature-continuous) natural cubic spline interpolation, solved
    /// once per channel via a global tridiagonal system (Thomas algorithm, natural
    /// zero-curvature boundary conditions), rather than v1's local Catmull-Rom cubic
    /// (C1-continuous, tangents from 4 neighboring samples). Promoted (2026-07-07) from the
    /// `natural_cubic_spline` experimental candidate after it beat v1 by 3-13 dB SNR on
    /// every gated fidelity scenario. Idea originally mined from gomplerate's
    /// <c>resample.go</c>. The retired v1 implementation is preserved at
    /// <c>Resampler/Experimental/Rejected/CatmullRomSpline.cs.old</c>.
    /// </remarks>
    public static class Spline
    {
        private static readonly SplineResampler _instance = new SplineResampler();

        public static float[] Resample(float[] inputData, int inRate, int outRate, int channels)
        {
            return _instance.Resample(inputData, inRate, outRate, channels);
        }

        public static float[] Resample(float[] inputData, WaveFormat inFormat, WaveFormat outFormat)
        {
            return _instance.Resample(inputData, inFormat, outFormat);
        }
    }

    /// <summary>
    /// Natural cubic spline resampler: one O(n) tridiagonal solve per channel up front buys
    /// a smoother, globally-consistent interpolant than a purely local cubic fit.
    /// </summary>
    public class SplineResampler : IAudioResampler
    {
        public float[] Resample(float[] inputData, WaveFormat inFormat, WaveFormat outFormat)
        {
            if (inFormat == null || outFormat == null)
                throw new ArgumentNullException("Both input and output WaveFormat must be provided.");

            return Resample(inputData, inFormat.SampleRate, outFormat.SampleRate, inFormat.Channels);
        }

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
            if (inRate == outRate)
                return (float[])inputData.Clone();

            int inputFrames = inputData.Length / channels;
            int outputFrames = (int)Math.Round(inputFrames * (double)outRate / inRate);
            if (outputFrames == 0)
                return Array.Empty<float>();

            double[][] channelData = SplitChannels(inputData, channels);
            var secondDerivatives = new double[channels][];
            Parallel.For(0, channels, c => secondDerivatives[c] = SolveSecondDerivatives(channelData[c]));

            var output = new float[outputFrames * channels];
            double ratio = inRate / (double)outRate;

            Parallel.For(0, outputFrames, i =>
            {
                double position = Math.Clamp(i * ratio, 0, inputFrames - 1);
                int segment = Math.Min((int)position, inputFrames - 2);
                double t = position - segment;

                for (int c = 0; c < channels; c++)
                    output[i * channels + c] = (float)EvaluateSegment(channelData[c], secondDerivatives[c], segment, t);
            });

            return output;
        }

        /// <summary>Evaluates the cubic segment between samples [i, i+1] using the solved second derivatives.</summary>
        private static double EvaluateSegment(double[] y, double[] m, int i, double t)
        {
            double a = 1 - t;
            double b = t;
            return a * y[i] + b * y[i + 1]
                + ((a * a * a - a) * m[i] + (b * b * b - b) * m[i + 1]) / 6.0;
        }

        /// <summary>
        /// Solves the natural cubic spline tridiagonal system (uniform unit spacing, natural
        /// boundary conditions m[0] = m[n-1] = 0) via the Thomas algorithm.
        /// </summary>
        private static double[] SolveSecondDerivatives(double[] y)
        {
            int n = y.Length;
            var m = new double[n];
            if (n < 3)
                return m; // Nothing to curve - a line (or point) has zero second derivative.

            int size = n - 2; // unknowns m[1..n-2]; m[0] and m[n-1] are fixed at 0.
            var diag = new double[size];
            var superDiag = new double[size];
            var rhs = new double[size];

            for (int k = 0; k < size; k++)
            {
                int i = k + 1;
                diag[k] = 4.0;
                superDiag[k] = 1.0;
                rhs[k] = 6.0 * (y[i + 1] - 2 * y[i] + y[i - 1]);
            }

            for (int k = 1; k < size; k++)
            {
                double factor = 1.0 / diag[k - 1]; // sub-diagonal coefficient is always 1
                diag[k] -= factor * superDiag[k - 1];
                rhs[k] -= factor * rhs[k - 1];
            }

            var solved = new double[size];
            solved[size - 1] = rhs[size - 1] / diag[size - 1];
            for (int k = size - 2; k >= 0; k--)
                solved[k] = (rhs[k] - superDiag[k] * solved[k + 1]) / diag[k];

            for (int k = 0; k < size; k++)
                m[k + 1] = solved[k];

            return m;
        }

        private static double[][] SplitChannels(float[] inputData, int channels)
        {
            int frames = inputData.Length / channels;
            var output = new double[channels][];
            for (int c = 0; c < channels; c++)
            {
                output[c] = new double[frames];
                for (int i = 0; i < frames; i++)
                    output[c][i] = inputData[i * channels + c];
            }
            return output;
        }
    }
}
