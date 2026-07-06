using MsgReader.Outlook;
using System.Text;

namespace Transformations.Text;

public class MsgExtractor : ITextExtractor
{
    public bool CanHandle(string extension) =>
        extension.Equals(".msg", StringComparison.OrdinalIgnoreCase);

    public string ExtractText(byte[] fileData)
    {
        using var stream = new MemoryStream(fileData);
        using var msg = new Storage.Message(stream);

        var sb = new StringBuilder();

        var sender = msg.Sender;
        if (sender != null)
        {
            var from = string.IsNullOrWhiteSpace(sender.DisplayName)
                ? sender.Email
                : $"{sender.DisplayName} <{sender.Email}>";
            if (!string.IsNullOrWhiteSpace(from))
                sb.AppendLine($"From: {from}");
        }

        if (msg.SentOn.HasValue)
            sb.AppendLine($"Date: {msg.SentOn.Value:R}");

        if (!string.IsNullOrWhiteSpace(msg.Subject))
            sb.AppendLine($"Subject: {msg.Subject}");

        if (sb.Length > 0)
            sb.AppendLine();

        var body = msg.BodyText;
        if (string.IsNullOrWhiteSpace(body) && !string.IsNullOrWhiteSpace(msg.BodyHtml))
        {
            var htmlExtractor = new HtmlExtractor();
            body = htmlExtractor.ExtractText(Encoding.UTF8.GetBytes(msg.BodyHtml));
        }

        if (!string.IsNullOrWhiteSpace(body))
            sb.Append(body);

        return sb.ToString().Trim();
    }
}
