// File: backend/Services/Course/CourseService.Infrastructure/Persistence/Repositories/EfLessonAiRepository.cs
// Mục đích: Adapter EF Core cho trạng thái trích xuất tài liệu và job AI của Lesson; nhận việc bằng UPDATE có điều kiện để nhiều worker không giẫm nhau.

using System.Text.Json;
using CourseService.Application.Repositories;
using CourseService.Domain.Constants;
using CourseService.Infrastructure.Persistence.Scaffolded;
using Microsoft.EntityFrameworkCore;

namespace CourseService.Infrastructure.Persistence.Repositories;

public sealed class EfLessonAiRepository(CourseDbContext db) : ILessonAiRepository
{
    private const int MaxClaimAttempts = 3;
    private const string InterruptedCode = "WORKER_INTERRUPTED";
    private const string InterruptedMessage = "The worker stopped before finishing. Try again.";

    public async Task<IReadOnlyList<LessonAiDocumentRecord>> GetDocumentsAsync(Guid lessonId, CancellationToken cancellationToken)
    {
        var rows = await db.LessonAttachmentTexts.AsNoTracking()
            .Where(item => item.LessonId == lessonId)
            .OrderBy(item => item.CreatedAt)
            .Select(item => new
            {
                item.Id,
                item.LessonId,
                item.MediaId,
                item.FileName,
                item.ContentType,
                item.Status,
                item.ErrorCode,
                item.ErrorMessage,
                TextLength = item.ExtractedText == null ? 0 : item.ExtractedText.Length,
                item.CreatedAt,
                item.UpdatedAt,
            })
            .ToListAsync(cancellationToken);
        return rows.Select(row => new LessonAiDocumentRecord(
            row.Id, row.LessonId, row.MediaId, row.FileName, row.ContentType, row.Status,
            row.ErrorCode, row.ErrorMessage, row.TextLength, Utc(row.CreatedAt), Utc(row.UpdatedAt))).ToArray();
    }

    public async Task<int> AddMissingDocumentsAsync(
        Guid lessonId,
        IReadOnlyList<NewLessonAiDocument> documents,
        Guid requestedBy,
        CancellationToken cancellationToken)
    {
        if (documents.Count == 0) return 0;
        var existing = (await db.LessonAttachmentTexts.AsNoTracking()
                .Where(item => item.LessonId == lessonId)
                .Select(item => item.MediaId)
                .ToListAsync(cancellationToken))
            .ToHashSet();
        var now = DateTime.UtcNow;
        var added = documents
            .Where(document => !existing.Contains(document.MediaId))
            .Select(document => new LessonAttachmentText
            {
                Id = Guid.NewGuid(),
                LessonId = lessonId,
                MediaId = document.MediaId,
                FileName = document.FileName,
                ContentType = document.ContentType,
                Status = document.Status,
                ErrorCode = document.ErrorCode,
                ErrorMessage = document.ErrorMessage,
                RequestedBy = requestedBy,
                CreatedAt = now,
                UpdatedAt = now,
            })
            .ToArray();
        if (added.Length == 0) return 0;

        db.LessonAttachmentTexts.AddRange(added);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.IsDuplicateKey())
        {
            // Hai lần sync đồng thời cùng thêm một tài liệu: bản ghi đã có người thêm, không phải lỗi.
            db.ChangeTracker.Clear();
            return 0;
        }

        return added.Length;
    }

    public Task<int> RetryFailedDocumentsAsync(Guid lessonId, IReadOnlyList<Guid> mediaIds, CancellationToken cancellationToken) =>
        db.LessonAttachmentTexts
            .Where(item => item.LessonId == lessonId && mediaIds.Contains(item.MediaId) && item.Status == LessonAiDocumentStatuses.Failed)
            .ExecuteUpdateAsync(
                set => set
                    .SetProperty(item => item.Status, LessonAiDocumentStatuses.Pending)
                    .SetProperty(item => item.ErrorCode, (string?)null)
                    .SetProperty(item => item.ErrorMessage, (string?)null)
                    .SetProperty(item => item.Attempts, 0)
                    .SetProperty(item => item.LockedUntil, (DateTime?)null),
                cancellationToken);

    public async Task<IReadOnlyList<LessonAiDocumentText>> GetDocumentTextsAsync(
        Guid lessonId,
        IReadOnlyList<Guid> mediaIds,
        CancellationToken cancellationToken) =>
        await db.LessonAttachmentTexts.AsNoTracking()
            .Where(item => item.LessonId == lessonId && mediaIds.Contains(item.MediaId))
            .OrderBy(item => item.CreatedAt)
            .Select(item => new LessonAiDocumentText(item.MediaId, item.FileName, item.Status, item.ExtractedText))
            .ToListAsync(cancellationToken);

    public async Task<LessonAiDocumentWork?> ClaimNextDocumentAsync(TimeSpan lease, int maxAttempts, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        // Worker chết giữa chừng: trả về hàng chờ nếu còn lượt thử, ngược lại đánh dấu lỗi để admin thử lại thủ công.
        await db.LessonAttachmentTexts
            .Where(item => item.Status == LessonAiDocumentStatuses.Extracting && item.LockedUntil < now && item.Attempts < maxAttempts)
            .ExecuteUpdateAsync(
                set => set.SetProperty(item => item.Status, LessonAiDocumentStatuses.Pending).SetProperty(item => item.LockedUntil, (DateTime?)null),
                cancellationToken);
        await db.LessonAttachmentTexts
            .Where(item => item.Status == LessonAiDocumentStatuses.Extracting && item.LockedUntil < now && item.Attempts >= maxAttempts)
            .ExecuteUpdateAsync(
                set => set
                    .SetProperty(item => item.Status, LessonAiDocumentStatuses.Failed)
                    .SetProperty(item => item.ErrorCode, InterruptedCode)
                    .SetProperty(item => item.ErrorMessage, InterruptedMessage)
                    .SetProperty(item => item.LockedUntil, (DateTime?)null),
                cancellationToken);

        for (var attempt = 0; attempt < MaxClaimAttempts; attempt++)
        {
            var candidate = await db.LessonAttachmentTexts.AsNoTracking()
                .Where(item => item.Status == LessonAiDocumentStatuses.Pending)
                .OrderBy(item => item.CreatedAt)
                .Select(item => new LessonAiDocumentWork(item.Id, item.LessonId, item.MediaId, item.FileName, item.ContentType, item.RequestedBy))
                .FirstOrDefaultAsync(cancellationToken);
            if (candidate is null) return null;

            var leaseUntil = now + lease;
            var claimed = await db.LessonAttachmentTexts
                .Where(item => item.Id == candidate.Id && item.Status == LessonAiDocumentStatuses.Pending)
                .ExecuteUpdateAsync(
                    set => set
                        .SetProperty(item => item.Status, LessonAiDocumentStatuses.Extracting)
                        .SetProperty(item => item.Attempts, item => item.Attempts + 1)
                        .SetProperty(item => item.LockedUntil, (DateTime?)leaseUntil),
                    cancellationToken);
            if (claimed == 1) return candidate;
        }

        return null;
    }

    public Task CompleteDocumentAsync(Guid documentId, string text, CancellationToken cancellationToken) =>
        db.LessonAttachmentTexts
            .Where(item => item.Id == documentId)
            .ExecuteUpdateAsync(
                set => set
                    .SetProperty(item => item.Status, LessonAiDocumentStatuses.Extracted)
                    .SetProperty(item => item.ExtractedText, text)
                    .SetProperty(item => item.ErrorCode, (string?)null)
                    .SetProperty(item => item.ErrorMessage, (string?)null)
                    .SetProperty(item => item.LockedUntil, (DateTime?)null),
                cancellationToken);

    public Task FailDocumentAsync(Guid documentId, string errorCode, string errorMessage, CancellationToken cancellationToken) =>
        db.LessonAttachmentTexts
            .Where(item => item.Id == documentId)
            .ExecuteUpdateAsync(
                set => set
                    .SetProperty(item => item.Status, LessonAiDocumentStatuses.Failed)
                    .SetProperty(item => item.ExtractedText, (string?)null)
                    .SetProperty(item => item.ErrorCode, Cut(errorCode, 60))
                    .SetProperty(item => item.ErrorMessage, Cut(errorMessage, 500))
                    .SetProperty(item => item.LockedUntil, (DateTime?)null),
                cancellationToken);

    public async Task<LessonAiJobRecord> CreateJobAsync(NewLessonAiJob job, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var entity = new LessonAiJob
        {
            Id = Guid.NewGuid(),
            LessonId = job.LessonId,
            CourseId = job.CourseId,
            JobType = job.JobType,
            Status = LessonAiJobStatuses.Pending,
            MediaIdsJson = JsonSerializer.Serialize(job.MediaIds),
            QuestionCount = job.QuestionCount,
            OverwriteContent = job.OverwriteContent,
            CreatedBy = job.CreatedBy,
            CreatedAt = now,
            UpdatedAt = now,
        };
        db.LessonAiJobs.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return ToRecord(new JobRow(
            entity.Id, entity.LessonId, entity.CourseId, entity.JobType, entity.Status, entity.MediaIdsJson,
            entity.QuestionCount, entity.OverwriteContent, false, null, 0, null, null,
            entity.CreatedBy, entity.CreatedAt, entity.UpdatedAt, null, null));
    }

    public Task<bool> HasActiveJobAsync(Guid lessonId, string jobType, CancellationToken cancellationToken) =>
        db.LessonAiJobs.AsNoTracking().AnyAsync(
            item => item.LessonId == lessonId
                && item.JobType == jobType
                && (item.Status == LessonAiJobStatuses.Pending || item.Status == LessonAiJobStatuses.Running),
            cancellationToken);

    public async Task<LessonAiJobRecord?> GetJobAsync(Guid lessonId, Guid jobId, CancellationToken cancellationToken)
    {
        var row = await ProjectJobs(db.LessonAiJobs.AsNoTracking().Where(item => item.LessonId == lessonId && item.Id == jobId))
            .FirstOrDefaultAsync(cancellationToken);
        return row is null ? null : ToRecord(row);
    }

    public Task<string?> GetJobPreviousContentAsync(Guid jobId, CancellationToken cancellationToken) =>
        db.LessonAiJobs.AsNoTracking()
            .Where(item => item.Id == jobId)
            .Select(item => item.PreviousContentMarkdown)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<LessonAiJobRecord>> GetRecentJobsAsync(Guid lessonId, int take, CancellationToken cancellationToken)
    {
        var rows = await ProjectJobs(db.LessonAiJobs.AsNoTracking().Where(item => item.LessonId == lessonId).OrderByDescending(item => item.CreatedAt).Take(take))
            .ToListAsync(cancellationToken);
        return rows.Select(ToRecord).ToArray();
    }

    public async Task<LessonAiJobRecord?> ClaimNextJobAsync(TimeSpan lease, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        // Job bị bỏ dở không chạy lại tự động: job có thể đã tạo Quiz hoặc ghi Lesson một phần, chạy lại sẽ tạo trùng.
        await db.LessonAiJobs
            .Where(item => item.Status == LessonAiJobStatuses.Running && item.LockedUntil < now)
            .ExecuteUpdateAsync(
                set => set
                    .SetProperty(item => item.Status, LessonAiJobStatuses.Failed)
                    .SetProperty(item => item.ErrorCode, InterruptedCode)
                    .SetProperty(item => item.ErrorMessage, InterruptedMessage)
                    .SetProperty(item => item.LockedUntil, (DateTime?)null)
                    .SetProperty(item => item.FinishedAt, (DateTime?)now),
                cancellationToken);

        for (var attempt = 0; attempt < MaxClaimAttempts; attempt++)
        {
            var candidateId = await db.LessonAiJobs.AsNoTracking()
                .Where(item => item.Status == LessonAiJobStatuses.Pending)
                .OrderBy(item => item.CreatedAt)
                .Select(item => (Guid?)item.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (candidateId is null) return null;

            var leaseUntil = now + lease;
            var claimed = await db.LessonAiJobs
                .Where(item => item.Id == candidateId && item.Status == LessonAiJobStatuses.Pending)
                .ExecuteUpdateAsync(
                    set => set
                        .SetProperty(item => item.Status, LessonAiJobStatuses.Running)
                        .SetProperty(item => item.LockedUntil, (DateTime?)leaseUntil)
                        .SetProperty(item => item.StartedAt, (DateTime?)now),
                    cancellationToken);
            if (claimed != 1) continue;

            var row = await ProjectJobs(db.LessonAiJobs.AsNoTracking().Where(item => item.Id == candidateId))
                .FirstAsync(cancellationToken);
            return ToRecord(row);
        }

        return null;
    }

    public Task RecordJobStepAsync(Guid jobId, int stepCount, TimeSpan lease, CancellationToken cancellationToken)
    {
        var leaseUntil = DateTime.UtcNow + lease;
        return db.LessonAiJobs
            .Where(item => item.Id == jobId)
            .ExecuteUpdateAsync(
                set => set.SetProperty(item => item.StepCount, stepCount).SetProperty(item => item.LockedUntil, (DateTime?)leaseUntil),
                cancellationToken);
    }

    public Task SetJobPreviousContentAsync(Guid jobId, string previousContent, CancellationToken cancellationToken) =>
        db.LessonAiJobs
            .Where(item => item.Id == jobId)
            .ExecuteUpdateAsync(set => set.SetProperty(item => item.PreviousContentMarkdown, previousContent), cancellationToken);

    public Task CompleteJobAsync(Guid jobId, Guid? resultQuizId, int stepCount, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        return db.LessonAiJobs
            .Where(item => item.Id == jobId)
            .ExecuteUpdateAsync(
                set => set
                    .SetProperty(item => item.Status, LessonAiJobStatuses.Completed)
                    .SetProperty(item => item.ResultQuizId, resultQuizId)
                    .SetProperty(item => item.StepCount, stepCount)
                    .SetProperty(item => item.ErrorCode, (string?)null)
                    .SetProperty(item => item.ErrorMessage, (string?)null)
                    .SetProperty(item => item.LockedUntil, (DateTime?)null)
                    .SetProperty(item => item.FinishedAt, (DateTime?)now),
                cancellationToken);
    }

    public Task FailJobAsync(Guid jobId, string errorCode, string errorMessage, int stepCount, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        return db.LessonAiJobs
            .Where(item => item.Id == jobId)
            .ExecuteUpdateAsync(
                set => set
                    .SetProperty(item => item.Status, LessonAiJobStatuses.Failed)
                    .SetProperty(item => item.StepCount, item => stepCount > 0 ? stepCount : item.StepCount)
                    .SetProperty(item => item.ErrorCode, Cut(errorCode, 60))
                    .SetProperty(item => item.ErrorMessage, Cut(errorMessage, 500))
                    .SetProperty(item => item.LockedUntil, (DateTime?)null)
                    .SetProperty(item => item.FinishedAt, (DateTime?)now),
                cancellationToken);
    }

    // Không đọc previous_content_markdown (MEDIUMTEXT) khi liệt kê job; chỉ cần biết có hay không.
    private static IQueryable<JobRow> ProjectJobs(IQueryable<LessonAiJob> source) =>
        source.Select(item => new JobRow(
            item.Id,
            item.LessonId,
            item.CourseId,
            item.JobType,
            item.Status,
            item.MediaIdsJson,
            item.QuestionCount,
            item.OverwriteContent,
            item.PreviousContentMarkdown != null,
            item.ResultQuizId,
            item.StepCount,
            item.ErrorCode,
            item.ErrorMessage,
            item.CreatedBy,
            item.CreatedAt,
            item.UpdatedAt,
            item.StartedAt,
            item.FinishedAt));

    private static LessonAiJobRecord ToRecord(JobRow row) =>
        new(
            row.Id,
            row.LessonId,
            row.CourseId,
            row.JobType,
            row.Status,
            JsonSerializer.Deserialize<Guid[]>(row.MediaIdsJson) ?? [],
            row.QuestionCount,
            row.OverwriteContent,
            row.HasPreviousContent,
            row.ResultQuizId,
            row.StepCount,
            row.ErrorCode,
            row.ErrorMessage,
            row.CreatedBy,
            Utc(row.CreatedAt),
            Utc(row.UpdatedAt),
            row.StartedAt is { } started ? Utc(started) : null,
            row.FinishedAt is { } finished ? Utc(finished) : null);

    private static DateTime Utc(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc);

    private static string Cut(string value, int maxLength) => value.Length <= maxLength ? value : value[..maxLength];

    private sealed record JobRow(
        Guid Id,
        Guid LessonId,
        Guid CourseId,
        string JobType,
        string Status,
        string MediaIdsJson,
        int? QuestionCount,
        bool OverwriteContent,
        bool HasPreviousContent,
        Guid? ResultQuizId,
        int StepCount,
        string? ErrorCode,
        string? ErrorMessage,
        Guid CreatedBy,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        DateTime? StartedAt,
        DateTime? FinishedAt);
}
