namespace Transformations.Audio.Channels;

/// <summary>
/// Selects the coefficient family used for mono or stereo downmixing.
/// </summary>
public enum DownmixCoefficientSet
{
    /// <summary>
    /// ITU-R BS.775-style Lo/Ro downmix. LFE is discarded and output is not headroom-normalized.
    /// </summary>
    ItuRbs775,

    /// <summary>
    /// ATSC A/85-style downmix with LFE folded in at -10 dB and global headroom normalization.
    /// </summary>
    AtscA85
}
