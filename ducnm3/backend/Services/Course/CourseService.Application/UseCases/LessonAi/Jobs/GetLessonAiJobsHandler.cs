// File: backend/Services/Course/CourseService.Application/UseCases/LessonAi/Jobs/GetLessonAiJobsHandler.cs
// Mục đích: Trả các job AI gần nhất của Lesson và chi tiết một job (kèm nội dung Lesson cũ nếu job đã ghi đè).

using CourseService.Application.Common.Errors;
using CourseService.Application.Repositories;

namespace CourseService.Application.UseCases.LessonAi.Jobs;

public sealed record LessonAiJobDetail(LessonAiJobRecord Job, string? PreviousContentMarkdown);

public sealed class GetLessonAiJobsHandler(ILessonCommandRepository lessonRepository, ILessonAiRepository repository)
{
    public const int RecentJobCount = 10;

    public async Task<IReadOnlyList<LessonAiJobRecord>> HandleListAsync(Guid courseId, Guid lessonId, CancellationToken cancellationToken)
    {
        await RequireLessonAsync(courseId, lessonId, cancellationToken);
        return await repository.GetRecentJobsAsync(lessonId, RecentJobCount, cancellationToken);
    }

    public async Task<LessonAiJobDetail> HandleDetailAsync(Guid courseId, Guid lessonId, Guid jobId, CancellationToken cancellationToken)
    {
        if (jobId == Guid.Empty) throw CourseErrors.ValidationFailed([]);
        await RequireLessonAsync(courseId, lessonId, cancellationToken);
        var job = await repository.GetJobAsync(lessonId, jobId, cancellationToken) ?? throw CourseErrors.LessonAiJobNotFound();
        var previous = job.HasPreviousContent ? await repository.GetJobPreviousContentAsync(jobId, cancellationToken) : null;
        return new LessonAiJobDetail(job, previous);
    }

    private async Task RequireLessonAsync(Guid courseId, Guid lessonId, CancellationToken cancellationToken)
    {
        if (courseId == Guid.Empty || lessonId == Guid.Empty) throw CourseErrors.ValidationFailed([]);
        _ = await lessonRepository.GetAsync(courseId, lessonId, cancellationToken) ?? throw CourseErrors.LessonNotFound();
    }
}
