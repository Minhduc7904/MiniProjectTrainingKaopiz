// File: backend/Services/Course/CourseService.Application/Repositories/ILessonAiRepository.cs
// Mục đích: Port lưu trạng thái trích xuất tài liệu và job AI của Lesson, kèm thao tác nhận việc (claim) cho worker.

namespace CourseService.Application.Repositories;

public sealed record LessonAiDocumentRecord(
    Guid Id,
    Guid LessonId,
    Guid MediaId,
    string FileName,
    string ContentType,
    string Status,
    string? ErrorCode,
    string? ErrorMessage,
    int TextLength,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

/// <summary>Tài liệu cần đưa vào hàng chờ; <see cref="Status"/> là PENDING, hoặc UNSUPPORTED kèm lý do.</summary>
public sealed record NewLessonAiDocument(
    Guid MediaId,
    string FileName,
    string ContentType,
    string Status,
    string? ErrorCode,
    string? ErrorMessage);

/// <summary>Việc trích xuất mà worker vừa nhận; <see cref="RequestedBy"/> là actor khi gọi AI Service.</summary>
public sealed record LessonAiDocumentWork(
    Guid Id,
    Guid LessonId,
    Guid MediaId,
    string FileName,
    string ContentType,
    Guid RequestedBy);

public sealed record LessonAiDocumentText(Guid MediaId, string FileName, string Status, string? Text);

public sealed record NewLessonAiJob(
    Guid LessonId,
    Guid CourseId,
    string JobType,
    IReadOnlyList<Guid> MediaIds,
    int? QuestionCount,
    bool OverwriteContent,
    Guid CreatedBy);

public sealed record LessonAiJobRecord(
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
    Guid CreatedBy,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime? StartedAtUtc,
    DateTime? FinishedAtUtc);

public interface ILessonAiRepository
{
    Task<IReadOnlyList<LessonAiDocumentRecord>> GetDocumentsAsync(Guid lessonId, CancellationToken cancellationToken);

    /// <summary>Thêm bản ghi cho tài liệu chưa có (không đụng bản ghi sẵn có); trả số bản ghi đã thêm.</summary>
    Task<int> AddMissingDocumentsAsync(
        Guid lessonId,
        IReadOnlyList<NewLessonAiDocument> documents,
        Guid requestedBy,
        CancellationToken cancellationToken);

    /// <summary>Đặt lại FAILED về PENDING cho các tài liệu <paramref name="mediaIds"/> của Lesson; trả số bản ghi đã đặt lại.</summary>
    Task<int> RetryFailedDocumentsAsync(Guid lessonId, IReadOnlyList<Guid> mediaIds, CancellationToken cancellationToken);

    Task<IReadOnlyList<LessonAiDocumentText>> GetDocumentTextsAsync(
        Guid lessonId,
        IReadOnlyList<Guid> mediaIds,
        CancellationToken cancellationToken);

    /// <summary>Nhận một tài liệu PENDING (hoặc EXTRACTING quá hạn lease) và đặt lease; null khi không có việc.</summary>
    Task<LessonAiDocumentWork?> ClaimNextDocumentAsync(TimeSpan lease, int maxAttempts, CancellationToken cancellationToken);

    Task CompleteDocumentAsync(Guid documentId, string text, CancellationToken cancellationToken);

    Task FailDocumentAsync(Guid documentId, string errorCode, string errorMessage, CancellationToken cancellationToken);

    Task<LessonAiJobRecord> CreateJobAsync(NewLessonAiJob job, CancellationToken cancellationToken);

    Task<bool> HasActiveJobAsync(Guid lessonId, string jobType, CancellationToken cancellationToken);

    Task<LessonAiJobRecord?> GetJobAsync(Guid lessonId, Guid jobId, CancellationToken cancellationToken);

    Task<string?> GetJobPreviousContentAsync(Guid jobId, CancellationToken cancellationToken);

    Task<IReadOnlyList<LessonAiJobRecord>> GetRecentJobsAsync(Guid lessonId, int take, CancellationToken cancellationToken);

    /// <summary>Nhận một job PENDING và đặt lease; job RUNNING quá hạn lease bị chuyển FAILED (không chạy lại để tránh tạo trùng Quiz).</summary>
    Task<LessonAiJobRecord?> ClaimNextJobAsync(TimeSpan lease, CancellationToken cancellationToken);

    /// <summary>Ghi số lượt gọi model và gia hạn lease để job dài không bị coi là bị bỏ dở.</summary>
    Task RecordJobStepAsync(Guid jobId, int stepCount, TimeSpan lease, CancellationToken cancellationToken);

    Task SetJobPreviousContentAsync(Guid jobId, string previousContent, CancellationToken cancellationToken);

    Task CompleteJobAsync(Guid jobId, Guid? resultQuizId, int stepCount, CancellationToken cancellationToken);

    Task FailJobAsync(Guid jobId, string errorCode, string errorMessage, int stepCount, CancellationToken cancellationToken);
}
