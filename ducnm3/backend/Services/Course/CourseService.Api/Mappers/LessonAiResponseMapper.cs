// File: backend/Services/Course/CourseService.Api/Mappers/LessonAiResponseMapper.cs
// Mục đích: Chuyển kết quả Application của tính năng AI cho Lesson sang response HTTP.

using CourseService.Api.Contracts.LessonAi;
using CourseService.Application.Repositories;
using CourseService.Application.UseCases.LessonAi.Documents;
using CourseService.Application.UseCases.LessonAi.Jobs;

namespace CourseService.Api.Mappers;

internal static class LessonAiResponseMapper
{
    public static LessonAiDocumentsResponse ToResponse(IReadOnlyList<LessonAiDocumentView> documents) =>
        new(documents.Select(document => new LessonAiDocumentResponse(
            document.MediaId,
            document.FileName,
            document.ContentType,
            document.MediaType,
            document.Status,
            document.ErrorCode,
            document.ErrorMessage,
            document.TextLength,
            document.UpdatedAtUtc)).ToArray());

    public static LessonAiJobResponse ToResponse(LessonAiJobRecord job) =>
        new(
            job.Id,
            job.LessonId,
            job.CourseId,
            job.JobType,
            job.Status,
            job.MediaIds,
            job.QuestionCount,
            job.OverwriteContent,
            job.HasPreviousContent,
            job.ResultQuizId,
            job.StepCount,
            job.ErrorCode,
            job.ErrorMessage,
            job.CreatedAtUtc,
            job.UpdatedAtUtc,
            job.StartedAtUtc,
            job.FinishedAtUtc);

    public static LessonAiJobsResponse ToResponse(IReadOnlyList<LessonAiJobRecord> jobs) =>
        new(jobs.Select(ToResponse).ToArray());

    public static LessonAiJobDetailResponse ToResponse(LessonAiJobDetail detail) =>
        new(ToResponse(detail.Job), detail.PreviousContentMarkdown);
}
