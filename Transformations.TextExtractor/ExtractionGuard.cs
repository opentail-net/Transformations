using System.Buffers;
using System.IO.Compression;
using System.Text;

namespace Transformations.Text;

/// <summary>
/// Thrown when a document would exceed a configured safety limit, such as
/// <see cref="ExtractionOptions.MaxDecompressedBytes"/> for an Office package.
/// The facade reports it as <see cref="ExtractionErrorKind.LimitExceeded"/>. When the document is an entry
/// inside a container, the container skips it with a warning instead: <see cref="ExtractionWarningCodes.ContainerEntryFailed"/>
/// for ZIP, <see cref="ExtractionWarningCodes.ContainerByteLimit"/> for EML attachments.
/// </summary>
public sealed class ExtractionLimitExceededException : Exception
{
    /// <summary>Creates the exception with a message describing the limit that was hit.</summary>
    public ExtractionLimitExceededException(string message) : base(message) { }
}

/// <summary>
/// Shared safety checks used by the built-in extractors: cooperative cancellation and
/// decompression limits for ZIP-based document formats.
/// </summary>
internal static class ExtractionGuard
{
    internal const long DefaultMaxDecompressedBytes = 512L * 1024 * 1024;
    internal const int DefaultMaxContainerEntries = 10_000;

    internal static void ThrowIfCancellationRequested(ExtractionOptions? options)
        => options?.CancellationToken.ThrowIfCancellationRequested();

    /// <summary>
    /// Reports the <paramref name="unit"/>s (pages, slides, chapters) of <paramref name="total"/> that fall outside
    /// <paramref name="start"/>..<paramref name="end"/>, so a selected subset is not taken for the whole document.
    /// </summary>
    internal static void ReportOmitted(ExtractionOptions? options, string source, string unit, int total, int start, int end)
    {
        if (options?.WarningSink is not { } sink || total <= 0) return;
        int first = Math.Max(1, start), last = Math.Min(total, end);
        int omitted = first > last ? total : (first - 1) + (total - last);
        if (omitted == 0) return;
        var kept = first > last ? "none" : first == last ? $"{unit} {first}" : $"{unit}s {first}-{last}";
        sink(new ExtractionWarning(ExtractionWarningCodes.PagesOmitted, source,
            $"{omitted} of {total} {unit}s not extracted; kept {kept} (StartPage/EndPage/MaxPages)."));
    }

    /// <summary>
    /// The entry count a ZIP's end-of-central-directory record declares (ZIP64 record when the classic field is
    /// saturated), or <c>null</c> when the record cannot be found. Read from the last 64 KB, without parsing the
    /// central directory. A lying count only decides this early check; the real count is checked after opening.
    /// </summary>
    internal static long? DeclaredEntryCount(byte[] data)
    {
        const int eocdSize = 22;
        int lowest = Math.Max(0, data.Length - eocdSize - ushort.MaxValue);
        for (int i = data.Length - eocdSize; i >= lowest; i--)
        {
            if (data[i] != 0x50 || data[i + 1] != 0x4B || data[i + 2] != 0x05 || data[i + 3] != 0x06) continue;
            long total = BitConverter.ToUInt16(data, i + 10);
            // ZIP64: the locator sits just before the classic record and points at the ZIP64 record (total at +32).
            if (total == ushort.MaxValue && i >= 20
                && data[i - 20] == 0x50 && data[i - 19] == 0x4B && data[i - 18] == 0x06 && data[i - 17] == 0x07)
            {
                long zip64At = (long)BitConverter.ToUInt64(data, i - 20 + 8);
                if (zip64At >= 0 && zip64At + 40 <= data.Length
                    && data[zip64At] == 0x50 && data[zip64At + 1] == 0x4B && data[zip64At + 2] == 0x06 && data[zip64At + 3] == 0x06)
                    total = (long)Math.Min(BitConverter.ToUInt64(data, (int)zip64At + 32), long.MaxValue);
            }
            return total;
        }
        return null;
    }

    internal static bool IsZip(byte[] data)
        => data.Length >= 4 && data[0] == 0x50 && data[1] == 0x4B && data[2] == 0x03 && data[3] == 0x04;

    /// <summary>
    /// Verifies that a ZIP-based document (DOCX, XLSX, PPTX, ODT, ...) stays within
    /// <see cref="ExtractionOptions.MaxContainerEntries"/> and <see cref="ExtractionOptions.MaxDecompressedBytes"/>
    /// before a parser library opens it. With a shared budget, both its parts and its bytes count against it. Declared sizes in the ZIP headers are checked first as a cheap
    /// reject, then every entry is actually decompressed into a counter, because headers can lie.
    /// Non-ZIP input is left alone.
    /// </summary>
    internal static void EnsurePackageWithinLimits(byte[] data, ExtractionOptions? options, string format)
    {
        if (!IsZip(data)) return;

        long maxBytes = options?.MaxDecompressedBytes ?? DefaultMaxDecompressedBytes;
        int maxEntries = options?.MaxContainerEntries ?? DefaultMaxContainerEntries;
        // A package inside a container spends the extraction's shared budget when there is one.
        var budget = options?.SharedBudget;
        long alreadyUsed = budget?.Used ?? 0;

        // Opening the archive reads its whole central directory, so a package that declares millions of parts is
        // rejected from its end-of-central-directory record first, before any entry objects are allocated.
        if (DeclaredEntryCount(data) is long declaredParts && declaredParts > maxEntries)
            throw new ExtractionLimitExceededException(
                $"{format} package declares {declaredParts} parts; the limit is {maxEntries}.");

        using var ms = new MemoryStream(data, writable: false);
        using var zip = new ZipArchive(ms, ZipArchiveMode.Read);

        if (zip.Entries.Count > maxEntries)
            throw new ExtractionLimitExceededException(
                $"{format} package has {zip.Entries.Count} parts; the limit is {maxEntries}.");
        if (budget != null)
        {
            // The package's parts spend the shared entry budget too, so a container of many small Office files cannot
            // give each one a fresh allowance.
            if (budget.Entries + zip.Entries.Count > maxEntries)
                throw new ExtractionLimitExceededException(
                    $"{format} package has {zip.Entries.Count} parts; with {budget.Entries} entries already processed in this extraction, that passes the shared limit of {maxEntries}.");
            budget.Entries += zip.Entries.Count;
        }

        long declared = alreadyUsed;
        foreach (var entry in zip.Entries)
        {
            declared += entry.Length;
            if (declared > maxBytes)
                throw new ExtractionLimitExceededException(
                    $"{format} package declares more than {maxBytes} uncompressed bytes.");
        }

        long total = alreadyUsed;
        var buffer = ArrayPool<byte>.Shared.Rent(81920);
        try
        {
            foreach (var entry in zip.Entries)
            {
                ThrowIfCancellationRequested(options);
                using var entryStream = entry.Open();
                int read;
                while ((read = entryStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    total += read;
                    if (total > maxBytes)
                        throw new ExtractionLimitExceededException(
                            $"{format} package expands to more than {maxBytes} bytes.");
                    ThrowIfCancellationRequested(options);
                }
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
            if (budget != null) budget.Used = total;
        }
    }

    /// <summary>
    /// Decompresses <paramref name="entry"/> against a budget shared by the whole container. Bytes are added to
    /// <paramref name="total"/> as they are read, so a failed or oversized entry still uses up the budget, whatever its
    /// header declared. Throws <see cref="ExtractionLimitExceededException"/> once <paramref name="total"/> passes
    /// <paramref name="maxBytes"/>; callers then stop processing the container.
    /// </summary>
    internal static byte[] ReadEntryWithBudget(ZipArchiveEntry entry, long maxBytes, ref long total, ExtractionOptions? options)
    {
        using var source = entry.Open();
        using var destination = new MemoryStream();
        var buffer = ArrayPool<byte>.Shared.Rent(81920);
        try
        {
            int read;
            while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                total += read;
                if (total > maxBytes)
                    throw new ExtractionLimitExceededException(
                        $"Container entries expanded past {maxBytes} decompressed bytes (ExtractionOptions.MaxDecompressedBytes).");
                destination.Write(buffer, 0, read);
                ThrowIfCancellationRequested(options);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
        return destination.ToArray();
    }

    /// <summary>
    /// Buffers a caller's stream for the public stream overloads of individual extractors, with the same input limit
    /// (<see cref="ExtractionOptions.MaxInputBytes"/>, default 256 MB) and cancellation checks as the facade.
    /// </summary>
    internal static byte[] ReadInput(Stream stream, ExtractionOptions? options)
    {
        long? limit = (options ?? ExtractionOptions.Default).MaxInputBytes;
        if (limit is long known && stream.CanSeek && stream.Length - stream.Position > known)
            throw new ExtractionLimitExceededException($"Input is larger than {known} bytes (ExtractionOptions.MaxInputBytes).");

        using var ms = new MemoryStream();
        var buffer = ArrayPool<byte>.Shared.Rent(81920);
        try
        {
            int read;
            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                ThrowIfCancellationRequested(options);
                if (limit is long max && ms.Length + read > max)
                    throw new ExtractionLimitExceededException($"Input is larger than {max} bytes (ExtractionOptions.MaxInputBytes).");
                ms.Write(buffer, 0, read);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
        return ms.ToArray();
    }

    /// <summary>Opens a ZIP entry for reading, failing once more than <paramref name="maxBytes"/> are read from it.</summary>
    internal static Stream OpenLimited(ZipArchiveEntry entry, long maxBytes)
        => new LimitedReadStream(entry.Open(), maxBytes);

    private sealed class LimitedReadStream : Stream
    {
        private readonly Stream _inner;
        private readonly long _maxBytes;
        private long _read;

        public LimitedReadStream(Stream inner, long maxBytes)
        {
            _inner = inner;
            _maxBytes = maxBytes;
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => _read; set => throw new NotSupportedException(); }

        public override int Read(byte[] buffer, int offset, int count) => Count(_inner.Read(buffer, offset, count));

        public override int Read(Span<byte> buffer) => Count(_inner.Read(buffer));

        private int Count(int read)
        {
            _read += read;
            if (_read > _maxBytes)
                throw new ExtractionLimitExceededException($"Archive entry expanded past {_maxBytes} bytes.");
            return read;
        }

        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing) _inner.Dispose();
            base.Dispose(disposing);
        }
    }
}

/// <summary>
/// Combines the caller's cancellation tokens with <see cref="ExtractionOptions.Timeout"/> for one
/// extraction, and remembers which of them fired.
/// </summary>
internal sealed class ExtractionCancellationScope : IDisposable
{
    private readonly CancellationToken _optionsToken;
    private readonly CancellationToken _callerToken;
    private readonly CancellationTokenSource? _timeout;
    private readonly CancellationTokenSource? _linked;

    public ExtractionCancellationScope(ExtractionOptions options, CancellationToken callerToken)
    {
        _optionsToken = options.CancellationToken;
        _callerToken = callerToken;
        if (options.Timeout is TimeSpan timeout)
            _timeout = new CancellationTokenSource(timeout);

        var tokens = new[] { _optionsToken, _callerToken, _timeout?.Token ?? default }
            .Where(t => t.CanBeCanceled)
            .Distinct()
            .ToArray();

        if (tokens.Length == 1)
        {
            Token = tokens[0];
        }
        else if (tokens.Length > 1)
        {
            _linked = CancellationTokenSource.CreateLinkedTokenSource(tokens);
            Token = _linked.Token;
        }
    }

    /// <summary>The token the extractors observe.</summary>
    public CancellationToken Token { get; }

    /// <summary>True when the caller, rather than the timeout, asked to stop.</summary>
    public bool CallerCancelled => _optionsToken.IsCancellationRequested || _callerToken.IsCancellationRequested;

    /// <summary>True when the timeout fired and the caller did not cancel.</summary>
    public bool TimedOut => _timeout?.IsCancellationRequested == true && !CallerCancelled;

    public void Dispose()
    {
        _linked?.Dispose();
        _timeout?.Dispose();
    }
}

/// <summary>
/// Writes the placeholder for a container part that was not extracted, so the reader of the text knows
/// it exists: name, why it was not read, detected type and size. Also reports the matching warning.
/// </summary>
internal static class PartStub
{
    internal static void Append(StringBuilder sb, string header, string source, string extension, byte[]? data,
        long size, string containerKind, ExtractionOptions? options)
    {
        string? sniffed = data != null ? ContentTypeDetector.SniffExtension(data) : null;
        if (data != null && ContentTypeDetector.DetectMismatch(extension, data) is { } detected)
            Warn(options, ExtractionWarningCodes.ContentTypeMismatch, source,
                $"{containerKind} extension '{extension}' does not match its content, which is '{detected}'.");

        string reason;
        if (ContentTypeDetector.IsImageExtension(sniffed))
        {
            reason = $"image ({sniffed}); text, if any, needs OCR";
            Warn(options, ExtractionWarningCodes.NeedsOcr, source, $"{containerKind} is an image ({sniffed}); no text was extracted.");
        }
        else
        {
            reason = data == null ? "could not be read" : $"no extractor for '{extension}'";
            if (sniffed != null && !string.Equals(sniffed, extension, StringComparison.OrdinalIgnoreCase))
                reason += $"; content is '{sniffed}'";
            Warn(options, ExtractionWarningCodes.ContainerUnsupportedEntry, source,
                $"No extractor is registered for {containerKind} extension '{extension}'.");
        }

        sb.AppendLine($"{header} not extracted: {reason}, {FormatSize(size)}");
        sb.AppendLine();
    }

    private static string FormatSize(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:0.#} KB",
        _ => $"{bytes / (1024.0 * 1024):0.#} MB",
    };

    private static void Warn(ExtractionOptions? options, string code, string source, string message)
        => options?.WarningSink?.Invoke(new ExtractionWarning(code, source, message));
}

/// <summary>
/// Decompression and entry budget shared by every container in one extraction when
/// <see cref="ExtractionOptions.ShareContainerBudget"/> is on, so nesting cannot reset the limits.
/// </summary>
internal sealed class ContainerBudget
{
    /// <summary>Bytes decompressed or decoded so far, failed entries included.</summary>
    public long Used;

    /// <summary>Container entries (ZIP entries, EML attachments, EPUB spine items, Office package parts) processed so far.</summary>
    public int Entries;
}
