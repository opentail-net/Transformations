using System.Buffers.Binary;

namespace Transformations.Text;

/// <summary>
/// Optical character recognition backend, set through <see cref="ExtractionOptions.OcrEngine"/>.
/// This package's <c>Transformations.Text.Ocr.TesseractOcrEngine</c> is the built-in implementation.
/// </summary>
public interface IOcrEngine
{
    /// <summary>
    /// <c>false</c> when the engine cannot run here (unsupported OS or architecture, missing language data).
    /// Images are then recorded as needing OCR and the engine is never called.
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Recognises text in an encoded image (PNG, JPEG, TIFF, ...). Called only for images whose header has been
    /// checked against <see cref="ExtractionOptions.MaxImagePixels"/>. Must be safe to call concurrently.
    /// </summary>
    OcrResult Recognize(byte[] image, CancellationToken cancellationToken);
}

/// <summary>Text recognised in one image. <paramref name="MeanConfidence"/> is 0..1 when the engine reports it.</summary>
public sealed record OcrResult(string Text, float? MeanConfidence = null);

/// <summary>
/// Reads text from raster images (PNG, JPEG, GIF, BMP, TIFF, WebP) through <see cref="ExtractionOptions.OcrEngine"/>.
/// The content must really be an image (checked by signature), and its pixel count is read from the header and
/// checked before anything decodes it. Without an available engine, extraction fails with
/// <see cref="ExtractionErrorKind.NeedsOcr"/>, and containers write a stub record instead.
/// OCR text is untrusted: images are an easy place to hide instructions aimed at an AI reader, so every OCR result
/// is reported with <see cref="ExtractionWarningCodes.OcrText"/>.
/// </summary>
public class ImageExtractor : ITextExtractor
{
    /// <inheritdoc />
    public bool CanHandle(string extension) => extension.ToLowerInvariant() is
        ".png" or ".jpg" or ".jpeg" or ".jpe" or ".gif" or ".bmp" or ".dib" or ".tif" or ".tiff" or ".webp";

    /// <inheritdoc />
    public string ExtractText(byte[] fileData) => ExtractText(fileData, null);

    /// <inheritdoc />
    public string ExtractText(byte[] fileData, ExtractionOptions? options)
    {
        var format = ContentTypeDetector.SniffExtension(fileData);
        if (!ContentTypeDetector.IsImageExtension(format))
            throw new InvalidDataException("Content is not a recognised image.");

        if (options?.OcrEngine is not { IsAvailable: true } engine)
            throw new OcrUnavailableException(options?.OcrEngine == null
                ? "OCR is not configured (ExtractionOptions.OcrEngine)"
                : "OCR is not available on this platform");

        return Recognize(fileData, format!, engine, options, $"image {format}");
    }

    /// <summary>True when OCR can run with these options.</summary>
    internal static bool OcrAvailable(ExtractionOptions? options) => options?.OcrEngine is { IsAvailable: true };

    /// <summary>Pixel check, then OCR, then the untrusted-text warning. Shared with scanned PDF pages.</summary>
    internal static string Recognize(byte[] image, string format, IOcrEngine engine, ExtractionOptions? options, string source)
    {
        long maxPixels = options?.MaxImagePixels ?? ExtractionOptions.Default.MaxImagePixels;
        if (ImageHeader.ReadSize(image) is not var (width, height))
            throw new ExtractionLimitExceededException($"Image dimensions ({format}) could not be read; not decoded.");
        if ((long)width * height > maxPixels)
            throw new ExtractionLimitExceededException(
                $"Image is {width}x{height} pixels; the limit is {maxPixels} (ExtractionOptions.MaxImagePixels).");

        ExtractionGuard.ThrowIfCancellationRequested(options);
        var result = engine.Recognize(image, options?.CancellationToken ?? default);
        options?.WarningSink?.Invoke(new ExtractionWarning(ExtractionWarningCodes.OcrText, source,
            $"Text was produced by OCR{(result.MeanConfidence is float c ? $" (confidence {c:0.00})" : "")}; treat it as untrusted."));
        return result.Text;
    }
}

/// <summary>Thrown when an image needs OCR and no engine can run; reported as <see cref="ExtractionErrorKind.NeedsOcr"/>.</summary>
internal sealed class OcrUnavailableException(string message) : Exception(message);

/// <summary>Reads image dimensions from file headers without decoding pixel data.</summary>
internal static class ImageHeader
{
    internal static (int Width, int Height)? ReadSize(byte[] d)
    {
        try
        {
            return ContentTypeDetector.SniffExtension(d) switch
            {
                ".png" => (BinaryPrimitives.ReadInt32BigEndian(d.AsSpan(16)), BinaryPrimitives.ReadInt32BigEndian(d.AsSpan(20))),
                ".gif" => (BinaryPrimitives.ReadUInt16LittleEndian(d.AsSpan(6)), BinaryPrimitives.ReadUInt16LittleEndian(d.AsSpan(8))),
                ".bmp" => BinaryPrimitives.ReadInt32LittleEndian(d.AsSpan(14)) == 12
                    ? (BinaryPrimitives.ReadUInt16LittleEndian(d.AsSpan(18)), BinaryPrimitives.ReadUInt16LittleEndian(d.AsSpan(20)))
                    : (Math.Abs(BinaryPrimitives.ReadInt32LittleEndian(d.AsSpan(18))), Math.Abs(BinaryPrimitives.ReadInt32LittleEndian(d.AsSpan(22)))),
                ".jpg" => Jpeg(d),
                ".tif" => Tiff(d),
                ".webp" => WebP(d),
                _ => null,
            } is var (w, h) && w > 0 && h > 0 ? (w, h) : null;
        }
        catch (ArgumentOutOfRangeException)
        {
            return null; // truncated header
        }
    }

    private static (int, int)? Jpeg(byte[] d)
    {
        int i = 2;
        while (i + 9 < d.Length)
        {
            if (d[i] != 0xFF) return null;
            byte marker = d[i + 1];
            if (marker == 0xFF) { i++; continue; }
            if (marker is 0x01 or (>= 0xD0 and <= 0xD9)) { i += 2; continue; }
            int length = BinaryPrimitives.ReadUInt16BigEndian(d.AsSpan(i + 2));
            // SOF0..SOF15 except DHT (C4), JPG (C8), DAC (CC) carry the frame size.
            if (marker is >= 0xC0 and <= 0xCF && marker is not (0xC4 or 0xC8 or 0xCC))
                return (BinaryPrimitives.ReadUInt16BigEndian(d.AsSpan(i + 7)), BinaryPrimitives.ReadUInt16BigEndian(d.AsSpan(i + 5)));
            i += 2 + length;
        }
        return null;
    }

    private static (int, int)? Tiff(byte[] d)
    {
        bool le = d[0] == 'I';
        uint U32(int o) => le ? BinaryPrimitives.ReadUInt32LittleEndian(d.AsSpan(o)) : BinaryPrimitives.ReadUInt32BigEndian(d.AsSpan(o));
        ushort U16(int o) => le ? BinaryPrimitives.ReadUInt16LittleEndian(d.AsSpan(o)) : BinaryPrimitives.ReadUInt16BigEndian(d.AsSpan(o));

        int ifd = checked((int)U32(4));
        int count = U16(ifd);
        int width = 0, height = 0;
        for (int e = 0; e < count; e++)
        {
            int at = ifd + 2 + e * 12;
            ushort tag = U16(at), type = U16(at + 2);
            int value = type == 3 ? U16(at + 8) : checked((int)U32(at + 8));
            if (tag == 256) width = value;
            else if (tag == 257) height = value;
        }
        return width > 0 && height > 0 ? (width, height) : null;
    }

    private static (int, int)? WebP(byte[] d)
    {
        var chunk = System.Text.Encoding.ASCII.GetString(d, 12, 4);
        return chunk switch
        {
            "VP8 " => (BinaryPrimitives.ReadUInt16LittleEndian(d.AsSpan(26)) & 0x3FFF, BinaryPrimitives.ReadUInt16LittleEndian(d.AsSpan(28)) & 0x3FFF),
            "VP8L" => (1 + (d[21] | ((d[22] & 0x3F) << 8)), 1 + ((d[22] >> 6) | (d[23] << 2) | ((d[24] & 0x0F) << 10))),
            "VP8X" => (1 + (d[24] | (d[25] << 8) | (d[26] << 16)), 1 + (d[27] | (d[28] << 8) | (d[29] << 16))),
            _ => null,
        };
    }
}
