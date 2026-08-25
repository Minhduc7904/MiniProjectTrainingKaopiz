// File: backend/Services/Course/CourseService.Application/UseCases/Courses/Export/CsvRowWriter.cs
// Mục đích: Ghi BOM, header và từng CSV row RFC 4180 trực tiếp vào stream.

using System.Text;

namespace CourseService.Application.UseCases.Courses.Export;

public static class CsvRowWriter
{
    // CSV payload dùng UTF-8 không BOM; BOM chỉ được WritePreambleAsync ghi một lần ở đầu file để Excel nhận đúng encoding.
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly byte[] Comma = [(byte)','];
    private static readonly byte[] CrLf = [(byte)'\r', (byte)'\n'];
    private const string Header = "id,name,status,createdAtUtc";

    public static async Task WritePreambleAsync(Stream stream, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(stream);
        // Preamble không phải dữ liệu CSV; nó đánh dấu UTF-8 cho client như Excel.
        await stream.WriteAsync(Encoding.UTF8.GetPreamble(), cancellationToken);
        await WriteValueAsync(stream, Header, cancellationToken);
        await stream.WriteAsync(CrLf, cancellationToken);
    }

    public static async Task WriteRowAsync(Stream stream, CourseExportRow row, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(row);

        // Không ghép cả file vào StringBuilder: serialize từng field rồi ghi ngay xuống stream của caller.
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
        // RFC 4180: chỉ quote field có ký tự đặc biệt; quote bên trong field được nhân đôi.
        return value.IndexOfAny([',', '"', '\r', '\n']) < 0
            ? value
            : $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
