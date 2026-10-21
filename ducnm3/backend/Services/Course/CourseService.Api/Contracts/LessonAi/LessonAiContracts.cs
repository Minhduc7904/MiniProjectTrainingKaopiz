// File: backend/Services/Course/CourseService.Api/Contracts/LessonAi/LessonAiContracts.cs
// Mục đích: Request/response HTTP của tài liệu AI và job AI trong Lesson.

namespace CourseService.Api.Contracts.LessonAi;

/// <summary><c>JobType</c> là SYNTHESIZE_LESSON hoặc GENERATE_QUIZ; <c>QuestionCount</c> chỉ dùng cho GENERATE_QUIZ; <c>OverwriteContent</c> cho phép ghi đè nội dung Lesson đã có.</summary>
public sealed record CreateLessonAiJobRequest(
    string? JobType,
    IReadOnlyList<Guid>? MediaIds,
    int? QuestionCount,
    bool? OverwriteContent);

public sealed record LessonAiDocumentResponse(
    Guid MediaId,
    string FileName,
    string ContentType,
    string MediaType,
    string Status,
    string? ErrorCode,
    string? ErrorMessage,
    int TextLength,
    DateTime? UpdatedAtUtc);

public sealed record LessonAiDocumentsResponse(IReadOnlyList<LessonAiDocumentResponse> Items);

public sealed record LessonAiJobResponse(
    Guid Id,
    Guid LessonId,
    Guid CourseId,
    string JobType,
    string Status,
    IReadOnlyList<Guid> MediaIds,
    int? QuestionCount,
    bool OverwriteContent,
    bool HasPreviousContent,
    Guid? ResultQuizId,
    int StepCount,
    string? ErrorCode,
    string? ErrorMessage,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime? StartedAtUtc,
    DateTime? FinishedAtUtc);

public sealed record LessonAiJobDetailResponse(LessonAiJobResponse Job, string? PreviousContentMarkdown);

public sealed record LessonAiJobsResponse(IReadOnlyList<LessonAiJobResponse> Items);
