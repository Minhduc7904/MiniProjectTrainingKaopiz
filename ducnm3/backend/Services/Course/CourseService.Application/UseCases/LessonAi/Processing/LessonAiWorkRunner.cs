// File: backend/Services/Course/CourseService.Application/UseCases/LessonAi/Processing/LessonAiWorkRunner.cs
// Mục đích: Nhận và xử lý một việc nền của Lesson AI (ưu tiên trích xuất tài liệu, sau đó job tổng hợp/quiz); lỗi bất ngờ được ghi vào bản ghi thay vì làm sập worker.

using CourseService.Application.Repositories;
using Microsoft.Extensions.Logging;

namespace CourseService.Application.UseCases.LessonAi.Processing;

public sealed class LessonAiWorkRunner(
    ILessonAiRepository repository,
    DocumentExtractionProcessor extractionProcessor,
    LessonAiJobProcessor jobProcessor,
    ILogger<LessonAiWorkRunner> logger)
{
    public const string InternalErrorCode = "INTERNAL_ERROR";
    public const string InternalErrorMessage = "An unexpected error stopped the AI processing. Try again later.";
    public const int MaxDocumentAttempts = 3;
    public static readonly TimeSpan DocumentLease = TimeSpan.FromMinutes(10);

    private static readonly Action<ILogger, string, Guid, Exception?> LogUnexpected = LoggerMessage.Define<string, Guid>(
        LogLevel.Error,
        new EventId(3001, "LessonAiWorkFailed"),
        "Unexpected failure while processing lesson AI {Kind} {Id}.");

    /// <returns>True khi đã nhận và xử lý một việc; false khi không còn việc để làm.</returns>
    public async Task<bool> RunNextAsync(CancellationToken cancellationToken)
    {
        var document = await repository.ClaimNextDocumentAsync(DocumentLease, MaxDocumentAttempts, cancellationToken);
        if (document is not null)
        {
            try
            {
                await extractionProcessor.ProcessAsync(document, cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                LogUnexpected(logger, "document", document.Id, exception);
                await repository.FailDocumentAsync(document.Id, InternalErrorCode, InternalErrorMessage, CancellationToken.None);
            }

            return true;
        }

        var job = await repository.ClaimNextJobAsync(LessonAiJobProcessor.Lease, cancellationToken);
        if (job is null) return false;

        try
        {
            await jobProcessor.ProcessAsync(job, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            LogUnexpected(logger, "job", job.Id, exception);
            await repository.FailJobAsync(job.Id, InternalErrorCode, InternalErrorMessage, 0, CancellationToken.None);
        }

        return true;
    }
}
