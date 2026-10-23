using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using TesseractOCR;
using TesseractOCR.Enums;

namespace Transformations.Text.Ocr;

/// <summary>
/// <see cref="IOcrEngine"/> backed by Tesseract 5 (via the TesseractOCR package). Runs only on Windows x64/x86, where
/// the native binaries ship with the package. Elsewhere, or when the language data is missing,
/// <see cref="IsAvailable"/> is <c>false</c>, no native code is loaded, and images are reported as needing OCR.
/// </summary>
/// <remarks>
/// Language data is not bundled: put <c>eng.traineddata</c> (and any other languages) from
/// https://github.com/tesseract-ocr/tessdata_fast in a folder and pass that folder. One Tesseract engine must not
/// be used by two threads at once, so engines are pooled; each concurrent call gets its own.
/// Tesseract's recognition call cannot be interrupted, so cancellation is checked before each image.
/// </remarks>
public sealed class TesseractOcrEngine : IOcrEngine, IDisposable
{
    private readonly string _tessdataPath;
    private readonly string _languages;
    private readonly ConcurrentBag<Engine> _pool = new();
    private volatile bool _disposed;

    /// <param name="tessdataPath">Folder containing <c>&lt;language&gt;.traineddata</c> files.</param>
    /// <param name="languages">Tesseract language codes joined with '+', e.g. "eng" or "eng+deu".</param>
    public TesseractOcrEngine(string tessdataPath, string languages = "eng")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tessdataPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(languages);
        _tessdataPath = tessdataPath;
        _languages = languages;
        UnavailableReason = FindUnavailableReason(tessdataPath, languages);
    }

    /// <summary>True when this process can run Tesseract: Windows x64/x86 and all language files present.</summary>
    public static bool IsPlatformSupported => OperatingSystem.IsWindows()
        && RuntimeInformation.ProcessArchitecture is Architecture.X64 or Architecture.X86;

    /// <inheritdoc />
    public bool IsAvailable => UnavailableReason == null && !_disposed;

    /// <summary>Why <see cref="IsAvailable"/> is false, or <c>null</c> when it is true.</summary>
    public string? UnavailableReason { get; }

    /// <inheritdoc />
    public OcrResult Recognize(byte[] image, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(image);
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (UnavailableReason != null)
            throw new PlatformNotSupportedException(UnavailableReason);
        cancellationToken.ThrowIfCancellationRequested();

        var engine = _pool.TryTake(out var pooled) ? pooled : new Engine(_tessdataPath, _languages, EngineMode.Default);
        try
        {
            using var pix = TesseractOCR.Pix.Image.LoadFromMemory(image);
            using var page = engine.Process(pix);
            return new OcrResult(page.Text ?? string.Empty, page.MeanConfidence);
        }
        finally
        {
            if (_disposed) engine.Dispose();
            else _pool.Add(engine);
        }
    }

    /// <summary>Disposes the pooled native engines.</summary>
    public void Dispose()
    {
        _disposed = true;
        while (_pool.TryTake(out var engine))
            engine.Dispose();
    }

    private static string? FindUnavailableReason(string tessdataPath, string languages)
    {
        if (!IsPlatformSupported)
            return $"Tesseract OCR runs only on Windows x64/x86 (this is {RuntimeInformation.OSDescription}, {RuntimeInformation.ProcessArchitecture}).";
        foreach (var lang in languages.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var file = Path.Combine(tessdataPath, lang + ".traineddata");
            if (!File.Exists(file))
                return $"Tesseract language data not found: {file}";
        }
        return null;
    }
}
