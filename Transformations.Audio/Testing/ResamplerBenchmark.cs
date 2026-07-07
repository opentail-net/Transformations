using NAudio.Transformations.Resampler;
using System.Diagnostics;

namespace NAudio.Transformations.Testing
{
    public static class ResamplerBenchmark
    {
        public static void CompareResamplingWithSNR(float[] inputData, int inRate, int outRate, int channels)
        {
            string results = "";

            double CalculateSNR(float[] original, float[] resampled)
            {
                double signalPower = 0, noisePower = 0;
                int minLength = Math.Min(original.Length, resampled.Length);
                for (int i = 0; i < minLength; i++)
                {
                    double diff = original[i] - resampled[i];
                    signalPower += original[i] * original[i];
                    noisePower += diff * diff;
                }
                return noisePower == 0 ? double.PositiveInfinity : 10 * Math.Log10(signalPower / noisePower);
            }

            double CalculatePSNR(float[] original, float[] resampled)
            {
                double mse = 0.0;
                int minLength = Math.Min(original.Length, resampled.Length);
                for (int i = 0; i < minLength; i++)
                {
                    double diff = original[i] - resampled[i];
                    mse += diff * diff;
                }
                mse /= minLength;
                double maxAmplitude = original.Max();
                return 20 * Math.Log10(maxAmplitude / Math.Sqrt(mse));
            }

            var resamplers = new (string Name, Func<float[], int, int, int, float[]> Resample)[]
            {
                //("Sinc1", Sinc.Resample),
                
                ("Experimental WDL_Sinc", Resampler.Experimental.WDL_Sinc.Resample),
                // Predictive and Hermite retired to Resampler/Experimental/Rejected/ - see
                // that folder's README.md (dominated by nearest_neighbor on both fidelity
                // and speed per the weighted composite scoring audit).
                //("Experimental Predictive", Resampler.Experimental.Predictive.Resample),
                ("Experimental Lanczos", Resampler.Experimental.Lanczos.Resample),
                //("Experimental Hermite", Resampler.Experimental.Hermite.Resample),
                ("Linear", Resampler.Linear.Resample),
                ("Spline", Resampler.Spline.Resample),
                ("Sinc", Resampler.Sinc.Resample),
                ("Hybrid", Resampler.Hybrid.Resample),
                //("FFT", Resampler.Experimental.FFT.Resample),
                ("Experimental Cubic", Resampler.Experimental.Cubic.Resample),
                ("Experimental NearestNeighbor", Resampler.Experimental.NearestNeighbor.Resample),
                


            };

            foreach (var resampler in resamplers)
            {
                var stopwatch = Stopwatch.StartNew();
                float[] resampled = resampler.Resample(inputData, inRate, outRate, channels);
                float[] doubleResampled = resampler.Resample(resampled, outRate, inRate, channels);
                stopwatch.Stop();

                // Calculate MSE
                double mse = 0;
                int minLength = Math.Min(inputData.Length, doubleResampled.Length);
                for (int i = 0; i < minLength; i++)
                {
                    double diff = inputData[i] - doubleResampled[i];
                    mse += diff * diff;
                }
                mse /= minLength;

                double snr = CalculateSNR(inputData, doubleResampled);
                double psnr = CalculatePSNR(inputData, doubleResampled);

                results += $"{resampler.Name}:";
                results += $"   Error: {mse},";
                results += $"   SNR   = {snr:F2} dB";
                results += $"   PSNR  = {psnr:F2} dB";
                results += $"   Time  = {stopwatch.ElapsedMilliseconds} ms\n";
            }

            Console.WriteLine("=== Resampler Comparison ===\n" + results);
        }
    }
}
