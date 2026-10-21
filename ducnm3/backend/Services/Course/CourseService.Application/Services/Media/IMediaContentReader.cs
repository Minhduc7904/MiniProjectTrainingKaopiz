// File: backend/Services/Course/CourseService.Application/Services/Media/IMediaContentReader.cs
// Mục đích: Port tải nội dung tệp từ Media Service để trích xuất văn bản, có giới hạn kích thước.

namespace CourseService.Application.Services.Media;

/// <summary>Lỗi khi tải tệp; <see cref="Code"/> ổn định, <see cref="Exception.Message"/> an toàn để hiển thị.</summary>
public sealed class MediaContentException(string code, string message) : Exception(message)
{
    public const string NotFound = "MEDIA_NOT_FOUND";
    public const string TooLarge = "MEDIA_TOO_LARGE";
    public const string Unavailable = "MEDIA_UNAVAILABLE";

    public string Code { get; } = code;
}

public interface IMediaContentReader
{
    /// <summary>Đọc toàn bộ nội dung; ném <see cref="MediaContentException"/> khi không có, quá <paramref name="maxBytes"/> hoặc Media Service lỗi.</summary>
    Task<byte[]> ReadAsync(Guid mediaId, long maxBytes, CancellationToken cancellationToken);
}
