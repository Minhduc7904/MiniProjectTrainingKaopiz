// File: backend/Services/Course/CourseService.Application/UseCases/LessonAi/Jobs/CreateLessonAiJobHandler.cs
// Mục đích: Tạo job AI (tổng hợp bài học hoặc soạn quiz) từ các tài liệu đã trích xuất mà admin chọn; worker nền sẽ chạy job.

using BuildingBlocks.Contracts.Api;
using CourseService.Application.Common.Errors;
using CourseService.Application.Repositories;
using CourseService.Domain.Constants;

namespace CourseService.Application.UseCases.LessonAi.Jobs;

public sealed record CreateLessonAiJobCommand(
    Guid CourseId,
    Guid LessonId,
    Guid ActorId,
    string? JobType,
    IReadOnlyList<Guid>? MediaIds,
    int? QuestionCount,
    bool OverwriteContent);

public sealed class CreateLessonAiJobHandler(ILessonCommandRepository lessonRepository, ILessonAiRepository repository)
{
    public const int MaxDocuments = 20;
    public const int DefaultQuestionCount = 10;
    public const int MaxQuestionCount = 30;

    public async Task<LessonAiJobRecord> HandleAsync(CreateLessonAiJobCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.CourseId == Guid.Empty || command.LessonId == Guid.Empty || command.ActorId == Guid.Empty)
        {
            throw CourseErrors.ValidationFailed([]);
        }

        var details = new List<ApiErrorDetail>();
        var jobType = command.JobType?.Trim().ToUpperInvariant();
        if (jobType is null || !LessonAiJobTypes.IsSupported(jobType))
        {
            details.Add(new ApiErrorDetail("jobType", $"jobType must be {LessonAiJobTypes.SynthesizeLesson} or {LessonAiJobTypes.GenerateQuiz}."));
        }

        var mediaIds = (command.MediaIds ?? []).Distinct().ToArray();
        if (mediaIds.Length == 0 || mediaIds.Length > MaxDocuments || mediaIds.Any(id => id == Guid.Empty))
        {
            details.Add(new ApiErrorDetail("mediaIds", $"Select between 1 and {MaxDocuments} documents."));
        }

        int? questionCount = null;
        if (jobType == LessonAiJobTypes.GenerateQuiz)
        {
            questionCount = command.QuestionCount ?? DefaultQuestionCount;
            if (questionCount is < 1 or > MaxQuestionCount)
            {
                details.Add(new ApiErrorDetail("questionCount", $"questionCount must be between 1 and {MaxQuestionCount}."));
            }
        }

        if (details.Count > 0) throw CourseErrors.ValidationFailed(details);

        var lesson = await lessonRepository.GetAsync(command.CourseId, command.LessonId, cancellationToken) ?? throw CourseErrors.LessonNotFound();
        if (jobType == LessonAiJobTypes.SynthesizeLesson
            && !command.OverwriteContent
            && !string.IsNullOrWhiteSpace(lesson.ContentMarkdown))
        {
            throw CourseErrors.LessonContentExists();
        }

        var texts = await repository.GetDocumentTextsAsync(command.LessonId, mediaIds, cancellationToken);
        if (texts.Count != mediaIds.Length || texts.Any(text => text.Status != LessonAiDocumentStatuses.Extracted))
        {
            throw CourseErrors.LessonAiDocumentNotReady();
        }

        if (await repository.HasActiveJobAsync(command.LessonId, jobType!, cancellationToken))
        {
            throw CourseErrors.LessonAiJobActive();
        }

        return await repository.CreateJobAsync(
            new NewLessonAiJob(command.LessonId, command.CourseId, jobType!, mediaIds, questionCount, command.OverwriteContent, command.ActorId),
            cancellationToken);
    }
}
