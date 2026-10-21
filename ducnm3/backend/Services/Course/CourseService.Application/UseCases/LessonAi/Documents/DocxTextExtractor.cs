// File: backend/Services/Course/CourseService.Application/UseCases/LessonAi/Documents/DocxTextExtractor.cs
// Mục đích: Đọc văn bản từ tệp DOCX (tiêu đề, danh sách, đoạn, bảng) thành Markdown mà không cần thư viện ngoài.

using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace CourseService.Application.UseCases.LessonAi.Documents;

public static class DocxTextExtractor
{
    private const long MaxDocumentXmlBytes = 50L * 1024 * 1024;
    private static readonly XNamespace W = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

    /// <summary>Trả Markdown của nội dung chính; ném <see cref="InvalidDataException"/> khi không phải DOCX hợp lệ.</summary>
    public static string Extract(byte[] content)
    {
        ArgumentNullException.ThrowIfNull(content);
        try
        {
            using var archive = new ZipArchive(new MemoryStream(content), ZipArchiveMode.Read);
            var entry = archive.GetEntry("word/document.xml")
                ?? throw new InvalidDataException("word/document.xml is missing.");
            if (entry.Length > MaxDocumentXmlBytes) throw new InvalidDataException("Document is too large.");

            using var stream = entry.Open();
            using var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
            var body = XDocument.Load(reader).Root?.Element(W + "body")
                ?? throw new InvalidDataException("Document body is missing.");

            var builder = new StringBuilder();
            foreach (var element in body.Elements())
            {
                if (element.Name == W + "p") AppendParagraph(builder, element);
                else if (element.Name == W + "tbl") AppendTable(builder, element);
            }

            return builder.ToString().Trim();
        }
        catch (Exception exception) when (exception is XmlException or InvalidDataException)
        {
            throw new InvalidDataException("File is not a valid DOCX document.", exception);
        }
    }

    private static void AppendParagraph(StringBuilder builder, XElement paragraph)
    {
        var text = ParagraphText(paragraph).Trim();
        if (text.Length == 0)
        {
            builder.Append('\n');
            return;
        }

        var style = paragraph.Element(W + "pPr")?.Element(W + "pStyle")?.Attribute(W + "val")?.Value ?? string.Empty;
        var isList = paragraph.Element(W + "pPr")?.Element(W + "numPr") is not null;
        if (style.Equals("Title", StringComparison.OrdinalIgnoreCase)) builder.Append("# ");
        else if (style.StartsWith("Heading", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(style["Heading".Length..], out var level))
        {
            builder.Append('#', Math.Clamp(level, 1, 6)).Append(' ');
        }
        else if (isList) builder.Append("- ");

        builder.Append(text).Append("\n\n");
    }

    private static void AppendTable(StringBuilder builder, XElement table)
    {
        var rows = table.Elements(W + "tr")
            .Select(row => row.Elements(W + "tc")
                .Select(cell => string.Join(" ", cell.Elements(W + "p").Select(p => ParagraphText(p).Trim()).Where(t => t.Length > 0)).Replace("|", "\\|", StringComparison.Ordinal))
                .ToArray())
            .Where(cells => cells.Length > 0)
            .ToArray();
        if (rows.Length == 0) return;

        var width = rows.Max(cells => cells.Length);
        for (var index = 0; index < rows.Length; index++)
        {
            var cells = rows[index].Concat(Enumerable.Repeat(string.Empty, width - rows[index].Length));
            builder.Append("| ").Append(string.Join(" | ", cells)).Append(" |\n");
            if (index == 0) builder.Append('|').Append(string.Concat(Enumerable.Repeat(" --- |", width))).Append('\n');
        }

        builder.Append('\n');
    }

    private static string ParagraphText(XElement paragraph)
    {
        var builder = new StringBuilder();
        foreach (var node in paragraph.Descendants())
        {
            if (node.Name == W + "t") builder.Append(node.Value);
            else if (node.Name == W + "tab") builder.Append('\t');
            else if (node.Name == W + "br" || node.Name == W + "cr") builder.Append('\n');
        }

        return builder.ToString();
    }
}
