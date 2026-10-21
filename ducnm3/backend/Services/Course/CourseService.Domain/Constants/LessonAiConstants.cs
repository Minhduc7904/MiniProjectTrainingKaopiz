// File: backend/Services/Course/CourseService.Domain/Constants/LessonAiConstants.cs
// Mục đích: Khai báo trạng thái trích xuất tài liệu và loại/trạng thái job AI của Lesson dùng thống nhất ở mọi layer.

namespace CourseService.Domain.Constants;

public static class LessonAiDocumentStatuses
{
    public const string Pending = "PENDING";
    public const string Extracting = "EXTRACTING";
    public const string Extracted = "EXTRACTED";
    public const string Failed = "FAILED";
    public const string Unsupported = "UNSUPPORTED";

    /// <summary>Trạng thái giả khi tài liệu mới đính kèm và chưa được đưa vào hàng chờ trích xuất.</summary>
    public const string NotQueued = "NOT_QUEUED";
}

public static class LessonAiJobTypes
{
    public const string SynthesizeLesson = "SYNTHESIZE_LESSON";
    public const string GenerateQuiz = "GENERATE_QUIZ";

    public static bool IsSupported(string value) => value is SynthesizeLesson or GenerateQuiz;
}

public static class LessonAiJobStatuses
{
    public const string Pending = "PENDING";
    public const string Running = "RUNNING";
    public const string Completed = "COMPLETED";
    public const string Failed = "FAILED";
}
