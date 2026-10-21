// File: backend/Services/Course/CourseService.Application/UseCases/LessonAi/Processing/LessonAiJobProcessor.cs
// Mục đích: Chạy một job AI của Lesson: gom văn bản các tài liệu đã chọn, chạy agent tổng hợp bài học hoặc soạn quiz, ghi kết quả/lỗi vào job.

using CourseService.Application.Repositories;
using CourseService.Application.Services.Ai;
using CourseService.Application.UseCases.Lessons.Update;
using CourseService.Application.UseCases.Quizzes.Create;
using CourseService.Domain.Constants;

namespace CourseService.Application.UseCases.LessonAi.Processing;

public sealed class LessonAiJobProcessor(
    ILessonAiRepository repository,
    ILessonCommandRepository lessonRepository,
    UpdateLessonHandler updateLessonHandler,
    CreateQuizHandler createQuizHandler,
    LessonAiAgentLoop agentLoop)
{
    public const string SynthesisTask = "lesson-synthesis";
    public const string QuizTask = "quiz-authoring";
    public const int MaxCorpusLength = 150_000;
    public static readonly TimeSpan Lease = TimeSpan.FromMinutes(10);

    public const string DocumentNotReadyCode = "DOCUMENT_NOT_READY";
    public const string DocumentsTooLargeCode = "DOCUMENTS_TOO_LARGE";

    public async Task ProcessAsync(LessonAiJobRecord job, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(job);
        var documents = await repository.GetDocumentTextsAsync(job.LessonId, job.MediaIds, cancellationToken);
        var ready = documents.Where(document => document.Status == LessonAiDocumentStatuses.Extracted && !string.IsNullOrEmpty(document.Text)).ToArray();
        if (ready.Length == 0 || ready.Length != job.MediaIds.Count)
        {
            await repository.FailJobAsync(job.Id, DocumentNotReadyCode, "Some selected documents have no extracted text yet.", 0, cancellationToken);
            return;
        }

        var corpus = LessonAiPrompts.BuildCorpus(ready.Select(document => (document.FileName, document.Text!)));
        if (corpus.Length > MaxCorpusLength)
        {
            await repository.FailJobAsync(
                job.Id,
                DocumentsTooLargeCode,
                $"The selected documents are too long ({corpus.Length} characters, limit {MaxCorpusLength}). Select fewer documents.",
                0,
                cancellationToken);
            return;
        }

        var isQuiz = job.JobType == LessonAiJobTypes.GenerateQuiz;
        var quizTools = isQuiz ? new CreateQuizToolSet(job, createQuizHandler) : null;
        ILessonAiToolSet tools = quizTools is not null
            ? quizTools
            : new SaveLessonContentToolSet(job, lessonRepository, repository, updateLessonHandler);

        LessonAiAgentResult result;
        try
        {
            result = await agentLoop.RunAsync(
                isQuiz ? QuizTask : SynthesisTask,
                isQuiz ? LessonAiPrompts.QuizSystem : LessonAiPrompts.SynthesisSystem,
                isQuiz ? LessonAiPrompts.QuizUser(corpus, job.QuestionCount ?? 10) : LessonAiPrompts.SynthesisUser(corpus),
                tools,
                job.CreatedBy,
                (steps, token) => repository.RecordJobStepAsync(job.Id, steps, Lease, token),
                cancellationToken);
        }
        catch (AiCompletionException exception)
        {
            await repository.FailJobAsync(job.Id, exception.Code, exception.Message, 0, cancellationToken);
            return;
        }

        if (result.Succeeded)
        {
            await repository.CompleteJobAsync(job.Id, quizTools?.QuizId, result.Steps, cancellationToken);
            return;
        }

        await repository.FailJobAsync(job.Id, result.FailureCode ?? LessonAiAgentLoop.NoResultCode, result.FailureMessage ?? "The job did not produce a result.", result.Steps, cancellationToken);
    }
}
