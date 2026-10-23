using System.IO.Compression;
using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using MimeKit;
using Transformations.Text;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;
using A = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Presentation;
using S = DocumentFormat.OpenXml.Spreadsheet;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace Transformations.Tests;

/// <summary>Small document builders shared by the safety corpus and model-input tests.</summary>
internal static class SafetyTestDocs
{
    public static readonly byte[] Png1x1 = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==");

    public static byte[] Utf8(string s) => Encoding.UTF8.GetBytes(s);

    public static string Tag(string ascii) => string.Concat(ascii.Select(c => char.ConvertFromUtf32(0xE0000 + c)));

    public static string Flag(string code) => char.ConvertFromUtf32(0x1F3F4) + Tag(code) + char.ConvertFromUtf32(0xE007F);

    public static byte[] Zip(params (string Name, byte[] Content)[] entries)
    {
        using var ms = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
            foreach (var (name, content) in entries)
            {
                using var s = zip.CreateEntry(name, CompressionLevel.NoCompression).Open();
                s.Write(content);
            }
        return ms.ToArray();
    }

    public static byte[] Eml(string? text, string? html, params (string Name, byte[] Content)[] attachments)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Sender", "sender@example.com"));
        message.To.Add(new MailboxAddress("Recipient", "to@example.com"));
        message.Subject = "Quarterly update";
        var body = new BodyBuilder { TextBody = text, HtmlBody = html };
        foreach (var (name, content) in attachments)
            body.Attachments.Add(name, content);
        message.Body = body.ToMessageBody();
        using var ms = new MemoryStream();
        message.WriteTo(ms);
        return ms.ToArray();
    }

    public static byte[] Docx(W.Styles? styles, params OpenXmlElement[] content)
    {
        using var ms = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document))
        {
            var main = doc.AddMainDocumentPart();
            main.Document = new W.Document(new W.Body(content));
            if (styles != null)
                main.AddNewPart<StyleDefinitionsPart>().Styles = styles;
        }
        return ms.ToArray();
    }

    public static W.Paragraph Para(string text, W.RunProperties? props = null)
        => new(props == null ? new W.Run(new W.Text(text)) : new W.Run(props, new W.Text(text)));

    /// <summary>One page per entry: visible text, optional invisible text, optional image area (x0, y0, x1, y1).</summary>
    public static byte[] Pdf(params (string Visible, string? Invisible, (double, double, double, double)? Image)[] pages)
    {
        var builder = new PdfDocumentBuilder();
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);
        foreach (var (visible, invisible, image) in pages)
        {
            var page = builder.AddPage(PageSize.A4);
            if (image is var (x0, y0, x1, y1))
                page.AddPng(Png1x1, new PdfRectangle(x0, y0, x1, y1));
            page.AddText(visible, 12, new PdfPoint(25, 750), font);
            if (invisible != null)
            {
                page.SetTextRenderingMode(TextRenderingMode.Neither);
                page.AddText(invisible, 12, new PdfPoint(25, 400), font);
                page.SetTextRenderingMode(TextRenderingMode.Fill);
            }
        }
        return builder.Build();
    }

    /// <summary>One shape per slide: (slide shown, text).</summary>
    public static byte[] Pptx(params (bool Shown, string Text)[] slides)
    {
        using var ms = new MemoryStream();
        using (var pres = PresentationDocument.Create(ms, PresentationDocumentType.Presentation))
        {
            var presPart = pres.AddPresentationPart();
            var ids = new P.SlideIdList();
            uint id = 256;
            foreach (var (shown, text) in slides)
            {
                var slidePart = presPart.AddNewPart<SlidePart>();
                var shape = new P.Shape(
                    new P.NonVisualShapeProperties(new P.NonVisualDrawingProperties { Id = 2U, Name = "T" },
                        new P.NonVisualShapeDrawingProperties(), new P.ApplicationNonVisualDrawingProperties()),
                    new P.ShapeProperties(),
                    new P.TextBody(new A.BodyProperties(), new A.Paragraph(new A.Run(new A.Text(text)))));
                var tree = new P.ShapeTree(
                    new P.NonVisualGroupShapeProperties(new P.NonVisualDrawingProperties { Id = 1U, Name = "" },
                        new P.NonVisualGroupShapeDrawingProperties(), new P.ApplicationNonVisualDrawingProperties()),
                    new P.GroupShapeProperties(), shape);
                slidePart.Slide = new P.Slide(new P.CommonSlideData(tree)) { Show = shown ? null : false };
                ids.Append(new P.SlideId { Id = id++, RelationshipId = presPart.GetIdOfPart(slidePart) });
            }
            presPart.Presentation = new P.Presentation(ids);
        }
        return ms.ToArray();
    }

    /// <summary>Sheets of (name, hidden, rows); each row is one cell.</summary>
    public static byte[] Xlsx(params (string Name, bool Hidden, string[] Rows)[] sheets)
    {
        using var ms = new MemoryStream();
        using (var doc = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook))
        {
            var wb = doc.AddWorkbookPart();
            wb.Workbook = new S.Workbook(new S.Sheets());
            uint sheetId = 1;
            foreach (var (name, hidden, rows) in sheets)
            {
                var part = wb.AddNewPart<WorksheetPart>();
                part.Worksheet = new S.Worksheet(new S.SheetData(rows.Select((value, i) => new S.Row(
                    new S.Cell { CellReference = $"A{i + 1}", DataType = S.CellValues.InlineString, InlineString = new S.InlineString(new S.Text(value)) })
                    { RowIndex = (uint)(i + 1) })));
                wb.Workbook.Sheets!.Append(new S.Sheet
                {
                    Name = name, SheetId = sheetId++, Id = wb.GetIdOfPart(part),
                    State = hidden ? S.SheetStateValues.Hidden : null,
                });
            }
        }
        return ms.ToArray();
    }

    public sealed class FakeOcr(string text) : IOcrEngine
    {
        public bool IsAvailable => true;
        public OcrResult Recognize(byte[] image, CancellationToken cancellationToken) => new(text, 0.9f);
    }
}
