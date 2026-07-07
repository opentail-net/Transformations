namespace Transformations.Audio.Analysis;

/// <summary>
/// Summary level measurements for interleaved audio.
/// </summary>
/// <param name="SamplePeak">The maximum absolute sample value.</param>
/// <param name="SamplePeakDbFs">The maximum absolute sample value in dBFS.</param>
/// <param name="TruePeak">The maximum FIR-estimated intersample peak value.</param>
/// <param name="TruePeakDbTp">The maximum FIR-estimated intersample peak in dBTP.</param>
/// <param name="Rms">The root mean square value across all samples.</param>
/// <param name="RmsDbFs">The root mean square value in dBFS.</param>
/// <param name="Frames">The number of complete interleaved frames measured.</param>
/// <param name="Channels">The channel count.</param>
public sealed record AudioLevelAnalysis(
    double SamplePeak,
    double SamplePeakDbFs,
    double TruePeak,
    double TruePeakDbTp,
    double Rms,
    double RmsDbFs,
    int Frames,
    int Channels);
