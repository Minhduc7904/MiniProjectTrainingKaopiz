// File: backend/Services/Ai/AiService.Domain/Constants/AiTaskNames.cs
// Mục đích: Khai báo khóa tác vụ AI dùng làm binding key, mỗi tác vụ gán một model riêng.

namespace AiService.Domain.Constants;

public static class AiTaskNames
{
    /// <summary>Trích xuất văn bản từ tài liệu đính kèm của Lesson (PDF, ảnh).</summary>
    public const string DocumentExtraction = "document-extraction";

    /// <summary>Tổng hợp tài liệu thành nội dung Markdown của Lesson.</summary>
    public const string LessonSynthesis = "lesson-synthesis";

    /// <summary>Soạn câu hỏi Quiz từ tài liệu của Lesson.</summary>
    public const string QuizAuthoring = "quiz-authoring";

    public static IReadOnlyList<string> All { get; } = [DocumentExtraction, LessonSynthesis, QuizAuthoring];
}
