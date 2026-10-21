// File: backend/Services/Course/CourseService.Application/UseCases/LessonAi/Documents/LessonAiDocumentKinds.cs
// Mục đích: Phân loại tệp đính kèm theo cách trích xuất văn bản (đọc thẳng, parser DOCX, LLM đọc PDF/ảnh) hoặc không hỗ trợ.

namespace CourseService.Application.UseCases.LessonAi.Documents;

public enum LessonAiDocumentKind
{
    Unsupported,
    Text,
    Docx,
    Pdf,
    Image,
}

public static class LessonAiDocumentKinds
{
    public const string UnsupportedErrorCode = "UNSUPPORTED_FILE_TYPE";
    public const string UnsupportedMessage = "File type is not supported for AI. Supported: PDF, DOCX, TXT, MD, PNG, JPG, WEBP, GIF.";

    private const string DocxContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

    public static LessonAiDocumentKind Classify(string? contentType, string? fileName)
    {
        var type = (contentType ?? string.Empty).Split(';')[0].Trim().ToLowerInvariant();
        var extension = Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant();

        if (type == DocxContentType || extension == ".docx") return LessonAiDocumentKind.Docx;
        if (type == "application/pdf" || extension == ".pdf") return LessonAiDocumentKind.Pdf;
        if (type is "image/png" or "image/jpeg" or "image/webp" or "image/gif"
            || extension is ".png" or ".jpg" or ".jpeg" or ".webp" or ".gif")
        {
            return LessonAiDocumentKind.Image;
        }

        if (type is "text/plain" or "text/markdown" or "text/x-markdown"
            || extension is ".txt" or ".md" or ".markdown")
        {
            return LessonAiDocumentKind.Text;
        }

        return LessonAiDocumentKind.Unsupported;
    }

    /// <summary>Media type gửi cho provider; ảnh/PDF lấy theo phần mở rộng khi Media Service không ghi đúng.</summary>
    public static string ResolveMediaType(LessonAiDocumentKind kind, string? contentType, string? fileName)
    {
        var type = (contentType ?? string.Empty).Split(';')[0].Trim().ToLowerInvariant();
        if (kind == LessonAiDocumentKind.Pdf) return "application/pdf";
        if (kind != LessonAiDocumentKind.Image) return type;
        if (type.StartsWith("image/", StringComparison.Ordinal)) return type;
        return Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => "image/jpeg",
        };
    }
}
