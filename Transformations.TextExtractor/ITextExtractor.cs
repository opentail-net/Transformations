namespace Transformations.Text;

// All implementations are now internal to the namespace
public interface ITextExtractor
{
    // Determines if this extractor can handle the specific file/data
    bool CanHandle(string extension);

    // The core execution logic
    string ExtractText(byte[] data);
}