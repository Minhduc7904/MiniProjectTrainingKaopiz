using CourseService.Application.Repositories;
using CourseService.Application.Services.Ai;
using CourseService.Application.Services.Media;
using CourseService.Domain.Constants;

namespace CourseService.UnitTests.Support;

/// <summary>Kho trạng thái trích xuất và job AI trong bộ nhớ, mô phỏng hợp đồng của repository EF (claim, lease, retry).</summary>
internal sealed class InMemoryLessonAiStore : ILessonAiRepository
{
    public List<LessonAiDocumentRecord> Documents { get; } = [];

    public Dictionary<Guid, string> Texts { get; } = [];

    public List<LessonAiJobRecord> Jobs { get; } = [];

    public Dictionary<Guid, string> PreviousContents { get; } = [];

    public Dictionary<Guid, Guid> DocumentRequesters { get; } = [];

    public List<string> Calls { get; } = [];

    public Task<IReadOnlyList<LessonAiDocumentRecord>> GetDocumentsAsync(Guid lessonId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<LessonAiDocumentRecord>>(Documents.Where(document => document.LessonId == lessonId).ToArray());

    public Task<int> AddMissingDocumentsAsync(Guid lessonId, IReadOnlyList<NewLessonAiDocument> documents, Guid requestedBy, CancellationToken cancellationToken)
    {
        var added = 0;
        foreach (var document in documents.Where(item => Documents.All(existing => existing.LessonId != lessonId || existing.MediaId != item.MediaId)))
        {
            var now = DateTime.UtcNow;
            var id = Guid.NewGuid();
            Documents.Add(new LessonAiDocumentRecord(id, lessonId, document.MediaId, document.FileName, document.ContentType, document.Status, document.ErrorCode, document.ErrorMessage, 0, now, now));
            DocumentRequesters[id] = requestedBy;
            added++;
        }

        return Task.FromResult(added);
    }

    public Task<int> RetryFailedDocumentsAsync(Guid lessonId, IReadOnlyList<Guid> mediaIds, CancellationToken cancellationToken)
    {
        var retried = 0;
        for (var index = 0; index < Documents.Count; index++)
        {
            var document = Documents[index];
            if (document.LessonId != lessonId || document.Status != LessonAiDocumentStatuses.Failed || !mediaIds.Contains(document.MediaId)) continue;
            Documents[index] = document with { Status = LessonAiDocumentStatuses.Pending, ErrorCode = null, ErrorMessage = null };
            retried++;
        }

        return Task.FromResult(retried);
    }

    public Task<IReadOnlyList<LessonAiDocumentText>> GetDocumentTextsAsync(Guid lessonId, IReadOnlyList<Guid> mediaIds, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<LessonAiDocumentText>>(Documents
            .Where(document => document.LessonId == lessonId && mediaIds.Contains(document.MediaId))
            .Select(document => new LessonAiDocumentText(document.MediaId, document.FileName, document.Status, Texts.GetValueOrDefault(document.Id)))
            .ToArray());

    public Task<LessonAiDocumentWork?> ClaimNextDocumentAsync(TimeSpan lease, int maxAttempts, CancellationToken cancellationToken)
    {
        for (var index = 0; index < Documents.Count; index++)
        {
            var document = Documents[index];
            if (document.Status != LessonAiDocumentStatuses.Pending) continue;
            Documents[index] = document with { Status = LessonAiDocumentStatuses.Extracting };
            return Task.FromResult<LessonAiDocumentWork?>(new LessonAiDocumentWork(
                document.Id, document.LessonId, document.MediaId, document.FileName, document.ContentType,
                DocumentRequesters.GetValueOrDefault(document.Id, Guid.NewGuid())));
        }

        return Task.FromResult<LessonAiDocumentWork?>(null);
    }

    public Task CompleteDocumentAsync(Guid documentId, string text, CancellationToken cancellationToken)
    {
        Update(documentId, document => document with { Status = LessonAiDocumentStatuses.Extracted, TextLength = text.Length, ErrorCode = null, ErrorMessage = null });
        Texts[documentId] = text;
        return Task.CompletedTask;
    }

    public Task FailDocumentAsync(Guid documentId, string errorCode, string errorMessage, CancellationToken cancellationToken)
    {
        Update(documentId, document => document with { Status = LessonAiDocumentStatuses.Failed, ErrorCode = errorCode, ErrorMessage = errorMessage });
        return Task.CompletedTask;
    }

    public Task<LessonAiJobRecord> CreateJobAsync(NewLessonAiJob job, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var record = new LessonAiJobRecord(
            Guid.NewGuid(), job.LessonId, job.CourseId, job.JobType, LessonAiJobStatuses.Pending, job.MediaIds, job.QuestionCount,
            job.OverwriteContent, false, null, 0, null, null, job.CreatedBy, now, now, null, null);
        Jobs.Add(record);
        return Task.FromResult(record);
    }

    public Task<bool> HasActiveJobAsync(Guid lessonId, string jobType, CancellationToken cancellationToken) =>
        Task.FromResult(Jobs.Any(job => job.LessonId == lessonId && job.JobType == jobType
            && job.Status is LessonAiJobStatuses.Pending or LessonAiJobStatuses.Running));

    public Task<LessonAiJobRecord?> GetJobAsync(Guid lessonId, Guid jobId, CancellationToken cancellationToken) =>
        Task.FromResult(Jobs.FirstOrDefault(job => job.Id == jobId && job.LessonId == lessonId));

    public Task<string?> GetJobPreviousContentAsync(Guid jobId, CancellationToken cancellationToken) =>
        Task.FromResult(PreviousContents.GetValueOrDefault(jobId));

    public Task<IReadOnlyList<LessonAiJobRecord>> GetRecentJobsAsync(Guid lessonId, int take, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<LessonAiJobRecord>>(Jobs.Where(job => job.LessonId == lessonId).OrderByDescending(job => job.CreatedAtUtc).Take(take).ToArray());

    public Task<LessonAiJobRecord?> ClaimNextJobAsync(TimeSpan lease, CancellationToken cancellationToken)
    {
        var index = Jobs.FindIndex(job => job.Status == LessonAiJobStatuses.Pending);
        if (index < 0) return Task.FromResult<LessonAiJobRecord?>(null);
        Jobs[index] = Jobs[index] with { Status = LessonAiJobStatuses.Running, StartedAtUtc = DateTime.UtcNow };
        return Task.FromResult<LessonAiJobRecord?>(Jobs[index]);
    }

    public Task RecordJobStepAsync(Guid jobId, int stepCount, TimeSpan lease, CancellationToken cancellationToken)
    {
        UpdateJob(jobId, job => job with { StepCount = stepCount });
        return Task.CompletedTask;
    }

    public Task SetJobPreviousContentAsync(Guid jobId, string previousContent, CancellationToken cancellationToken)
    {
        PreviousContents[jobId] = previousContent;
        UpdateJob(jobId, job => job with { HasPreviousContent = true });
        return Task.CompletedTask;
    }

    public Task CompleteJobAsync(Guid jobId, Guid? resultQuizId, int stepCount, CancellationToken cancellationToken)
    {
        UpdateJob(jobId, job => job with { Status = LessonAiJobStatuses.Completed, ResultQuizId = resultQuizId, StepCount = stepCount, FinishedAtUtc = DateTime.UtcNow });
        return Task.CompletedTask;
    }

    public Task FailJobAsync(Guid jobId, string errorCode, string errorMessage, int stepCount, CancellationToken cancellationToken)
    {
        UpdateJob(jobId, job => job with { Status = LessonAiJobStatuses.Failed, ErrorCode = errorCode, ErrorMessage = errorMessage, StepCount = stepCount, FinishedAtUtc = DateTime.UtcNow });
        return Task.CompletedTask;
    }

    /// <summary>Thêm tài liệu đã trích xuất sẵn để test job không phải chạy bước trích xuất.</summary>
    public LessonAiDocumentRecord AddExtracted(Guid lessonId, string fileName, string text, Guid? mediaId = null)
    {
        var now = DateTime.UtcNow;
        var record = new LessonAiDocumentRecord(Guid.NewGuid(), lessonId, mediaId ?? Guid.NewGuid(), fileName, "text/plain", LessonAiDocumentStatuses.Extracted, null, null, text.Length, now, now);
        Documents.Add(record);
        Texts[record.Id] = text;
        return record;
    }

    private void Update(Guid documentId, Func<LessonAiDocumentRecord, LessonAiDocumentRecord> change)
    {
        var index = Documents.FindIndex(document => document.Id == documentId);
        if (index >= 0) Documents[index] = change(Documents[index]);
    }

    private void UpdateJob(Guid jobId, Func<LessonAiJobRecord, LessonAiJobRecord> change)
    {
        var index = Jobs.FindIndex(job => job.Id == jobId);
        if (index >= 0) Jobs[index] = change(Jobs[index]);
    }
}

/// <summary>Model giả chạy theo kịch bản: mỗi lần gọi trả bước kế tiếp và ghi lại toàn bộ request để test kiểm tra.</summary>
internal sealed class ScriptedAiCompletionClient(params Func<AiCompletion>[] steps) : IAiCompletionClient
{
    private int _next;

    public List<(string Task, IReadOnlyList<AiMessage> Messages, IReadOnlyList<AiToolDefinition> Tools, Guid ActorId)> Requests { get; } = [];

    public Task<AiCompletion> CompleteAsync(string task, IReadOnlyList<AiMessage> messages, IReadOnlyList<AiToolDefinition> tools, Guid actorId, CancellationToken cancellationToken)
    {
        // Sao chép danh sách vì agent loop tiếp tục thêm message vào cùng một list sau mỗi lượt.
        Requests.Add((task, messages.ToArray(), tools, actorId));
        if (_next >= steps.Length) throw new InvalidOperationException("The script has no more steps.");
        return Task.FromResult(steps[_next++]());
    }

    public static AiCompletion Text(string text, string finishReason = "stop") => new(text, [], finishReason, 10, 20);

    public static AiCompletion Tool(string name, string argumentsJson, string id = "call_1") =>
        new(null, [new AiToolCall(id, name, argumentsJson)], "tool_calls", 10, 20);
}

internal sealed class ThrowingAiCompletionClient(AiCompletionException exception) : IAiCompletionClient
{
    public Task<AiCompletion> CompleteAsync(string task, IReadOnlyList<AiMessage> messages, IReadOnlyList<AiToolDefinition> tools, Guid actorId, CancellationToken cancellationToken) =>
        throw exception;
}

internal sealed class FakeMediaContentReader : IMediaContentReader
{
    public Dictionary<Guid, byte[]> Files { get; } = [];

    public Dictionary<Guid, MediaContentException> Failures { get; } = [];

    public Task<byte[]> ReadAsync(Guid mediaId, long maxBytes, CancellationToken cancellationToken)
    {
        if (Failures.TryGetValue(mediaId, out var failure)) throw failure;
        return Files.TryGetValue(mediaId, out var content) ? Task.FromResult(content) : throw new MediaContentException(MediaContentException.NotFound, "missing");
    }
}

internal sealed class FakeCourseMediaReader : ICourseMediaReader
{
    public List<CourseMediaAsset> Attachments { get; } = [];

    public Task<IReadOnlyList<CourseMediaAsset>> GetLessonAttachmentsAsync(Guid lessonId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<CourseMediaAsset>>(Attachments.Where(asset => asset.OwnerId == lessonId).ToArray());

    public Task<CourseMediaSet> GetAsync(Guid courseId, CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task<IReadOnlyDictionary<Guid, CourseMediaSet>> GetManyAsync(IReadOnlyList<Guid> courseIds, CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task<IReadOnlyList<Guid>> GetActiveUsageIdsAsync(IReadOnlyList<CourseMediaUsageOwnerScope> owners, CancellationToken cancellationToken) => throw new NotSupportedException();

    public CourseMediaAsset Add(Guid lessonId, string fileName, string contentType, uint order = 1, Guid? mediaId = null)
    {
        var asset = new CourseMediaAsset(Guid.NewGuid(), mediaId ?? Guid.NewGuid(), lessonId, "/content", null, null, order, "DOCUMENT", contentType, fileName);
        Attachments.Add(asset);
        return asset;
    }
}

/// <summary>Kho Lesson trong bộ nhớ cho phép đọc và ghi nội dung, dùng cho tool lưu nội dung bài học.</summary>
internal sealed class MutableLessonRepository(Guid courseId, Guid lessonId, string? content = null) : ILessonCommandRepository
{
    public string? Content { get; private set; } = content;

    public void SetContent(string value) => Content = value;

    public Task<LessonCreateRecord?> GetAsync(Guid course, Guid lesson, CancellationToken cancellationToken) =>
        Task.FromResult<LessonCreateRecord?>(course == courseId && lesson == lessonId
            ? new LessonCreateRecord(lessonId, courseId, "Lesson", Content, 1, DateTime.UtcNow, DateTime.UtcNow)
            : null);

    public Task<LessonCreateRecord?> UpdateAsync(Guid course, Guid lesson, string title, string? contentMarkdown, CancellationToken cancellationToken)
    {
        if (course != courseId || lesson != lessonId) return Task.FromResult<LessonCreateRecord?>(null);
        Content = contentMarkdown;
        return GetAsync(course, lesson, cancellationToken);
    }

    public Task<LessonCreateRecord?> CreateAsync(Guid course, string title, string? contentMarkdown, uint? displayOrder, CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task<bool> ReorderAsync(Guid course, IReadOnlyList<Guid> lessonIds, CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task<bool> DeleteAsync(Guid course, Guid lesson, CancellationToken cancellationToken) => throw new NotSupportedException();
}
