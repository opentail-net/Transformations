namespace Transformations
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;

    /// <summary>
    /// High-visibility process diagnostics metrics.
    /// </summary>
    public sealed class ProcessMetrics
    {
        /// <summary>
        /// Gets or sets the CPU usage percentage for the current process.
        /// Returns -1 when unavailable.
        /// </summary>
        public double CpuUsagePercent { get; set; }

        /// <summary>
        /// Gets or sets the private memory size in MB.
        /// Returns -1 when unavailable.
        /// </summary>
        public double PrivateMemoryMb { get; set; }

        /// <summary>
        /// Gets or sets the thread count for the current process.
        /// Returns -1 when unavailable.
        /// </summary>
        public int ThreadCount { get; set; }

        /// <summary>
        /// Gets or sets available GPU VRAM in MB (if discoverable).
        /// Returns -1 when unavailable.
        /// </summary>
        public double AvailableVramMb { get; set; }
    }

    /// <summary>
    /// Lightweight diagnostics probe for process and GPU metrics.
    /// </summary>
    public static class DiagnosticsProbe
    {
        /// <summary>
        /// Returns current process metrics with high-visibility fallback behavior.
        /// Any unavailable metric is returned as -1.
        /// </summary>
        /// <returns>The current process metrics.</returns>
        public static ProcessMetrics GetProcessMetrics()
        {
            var metrics = new ProcessMetrics
            {
                CpuUsagePercent = -1,
                PrivateMemoryMb = -1,
                ThreadCount = -1,
                AvailableVramMb = -1,
            };

            try
            {
                using Process process = Process.GetCurrentProcess();

                try
                {
                    TimeSpan cpuTime = process.TotalProcessorTime;
                    DateTime startTime = process.StartTime;
                    double elapsedSeconds = (DateTime.Now - startTime).TotalSeconds;
                    if (elapsedSeconds > 0)
                    {
                        double cpuPercent = (cpuTime.TotalSeconds / (elapsedSeconds * Environment.ProcessorCount)) * 100d;
                        if (cpuPercent < 0)
                        {
                            cpuPercent = 0;
                        }

                        if (cpuPercent > 100)
                        {
                            cpuPercent = 100;
                        }

                        metrics.CpuUsagePercent = Math.Round(cpuPercent, 2);
                    }
                }
                catch
                {
                    metrics.CpuUsagePercent = -1;
                }

                try
                {
                    metrics.PrivateMemoryMb = Math.Round(process.PrivateMemorySize64 / 1024d / 1024d, 2);
                }
                catch
                {
                    metrics.PrivateMemoryMb = -1;
                }

                try
                {
                    metrics.ThreadCount = process.Threads.Count;
                }
                catch
                {
                    metrics.ThreadCount = -1;
                }
            }
            catch
            {
                // Keep defaults (-1)
            }

            metrics.AvailableVramMb = GetAvailableVramMb();
            return metrics;
        }

        private static double GetAvailableVramMb()
        {
            try
            {
                // NVIDIA: query free memory per GPU (MB)
                string? nvidiaOutput = TryExecute("nvidia-smi", "--query-gpu=memory.free --format=csv,noheader,nounits");
                if (!string.IsNullOrWhiteSpace(nvidiaOutput))
                {
                    double totalFree = 0;
                    bool parsedAny = false;
                    foreach (string line in nvidiaOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (double.TryParse(line.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                        {
                            totalFree += value;
                            parsedAny = true;
                        }
                    }

                    if (parsedAny)
                    {
                        return Math.Round(totalFree, 2);
                    }
                }

                // AMD/ROCm (Linux): parse rocm-smi VRAM output, using "vram Total Used"
                string? rocmOutput = TryExecute("rocm-smi", "--showmeminfo vram");
                if (!string.IsNullOrWhiteSpace(rocmOutput))
                {
                    // Example line pattern often contains two numbers: total and used bytes.
                    // We compute free = total - used and sum across adapters.
                    double freeBytesTotal = 0;
                    foreach (string line in rocmOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        string normalized = line.Trim();
                        if (!normalized.Contains("vram", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        var numbers = normalized
                            .Split(new[] { ' ', '\t', ':', ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(token =>
                            {
                                bool ok = double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out double n);
                                return (ok, n);
                            })
                            .Where(x => x.ok)
                            .Select(x => x.n)
                            .ToArray();

                        if (numbers.Length >= 2)
                        {
                            double totalBytes = numbers[0];
                            double usedBytes = numbers[1];
                            if (totalBytes >= usedBytes)
                            {
                                freeBytesTotal += (totalBytes - usedBytes);
                            }
                        }
                    }

                    if (freeBytesTotal > 0)
                    {
                        return Math.Round(freeBytesTotal / 1024d / 1024d, 2);
                    }
                }

                // Windows fallback for AMD/NVIDIA presence via AdapterRAM (total, not free)
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    string? wmicOutput = TryExecute("wmic", "path win32_VideoController get AdapterRAM");
                    if (!string.IsNullOrWhiteSpace(wmicOutput))
                    {
                        double maxAdapterRam = -1;
                        foreach (string line in wmicOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (double.TryParse(line.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double bytes) && bytes > maxAdapterRam)
                            {
                                maxAdapterRam = bytes;
                            }
                        }

                        if (maxAdapterRam > 0)
                        {
                            // High-visibility compromise: return total VRAM when free VRAM is not exposed by native tools.
                            return Math.Round(maxAdapterRam / 1024d / 1024d, 2);
                        }
                    }
                }
            }
            catch
            {
                return -1;
            }

            return -1;
        }

        private static string? TryExecute(string fileName, string arguments)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Directory.GetCurrentDirectory(),
                };

                using Process? process = Process.Start(startInfo);
                if (process == null)
                {
                    return null;
                }

                if (!process.WaitForExit(1500))
                {
                    try
                    {
                        process.Kill(entireProcessTree: true);
                    }
                    catch
                    {
                    }

                    return null;
                }

                string output = process.StandardOutput.ReadToEnd();
                return string.IsNullOrWhiteSpace(output) ? null : output;
            }
            catch
            {
                return null;
            }
        }
    }
}
