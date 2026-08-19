// File: backend/Services/Course/CourseService.Application/UseCases/Courses/Export/CsvRowWriter.cs
// Mục đích: Ghi BOM, header và từng CSV row RFC 4180 trực tiếp vào stream.

using System.Text;

namespace CourseService.Application.UseCases.Courses.Export;

public static class CsvRowWriter
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly byte[] Comma = [(byte)','];
    private static readonly byte[] CrLf = [(byte)'\r', (byte)'\n'];
    private const string Header = "id,name,status,createdAtUtc";

    public static async Task WritePreambleAsync(Stream stream, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(stream);
        await stream.WriteAsync(Encoding.UTF8.GetPreamble(), cancellationToken);
        await WriteValueAsync(stream, Header, cancellationToken);
        await stream.WriteAsync(CrLf, cancellationToken);
    }

    public static async Task WriteRowAsync(Stream stream, CourseExportRow row, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(row);

        await WriteFieldAsync(stream, row.Id.ToString(), cancellationToken);
        await stream.WriteAsync(Comma, cancellationToken);
        await WriteFieldAsync(stream, row.Name, cancellationToken);
        await stream.WriteAsync(Comma, cancellationToken);
        await WriteFieldAsync(stream, row.Status, cancellationToken);
        await stream.WriteAsync(Comma, cancellationToken);
        await WriteFieldAsync(stream, row.CreatedAtUtc.ToUniversalTime().ToString("O"), cancellationToken);
        await stream.WriteAsync(CrLf, cancellationToken);
    }

    private static Task WriteFieldAsync(Stream stream, string value, CancellationToken cancellationToken) =>
        WriteValueAsync(stream, Escape(value), cancellationToken);

    private static Task WriteValueAsync(Stream stream, string value, CancellationToken cancellationToken) =>
        stream.WriteAsync(Utf8WithoutBom.GetBytes(value), cancellationToken).AsTask();

    private static string Escape(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.IndexOfAny([',', '"', '\r', '\n']) < 0
            ? value
            : $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
