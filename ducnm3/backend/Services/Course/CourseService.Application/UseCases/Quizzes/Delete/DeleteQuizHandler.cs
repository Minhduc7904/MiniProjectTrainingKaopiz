// File: backend/Services/Course/CourseService.Application/UseCases/Quizzes/Delete/DeleteQuizHandler.cs
// Mục đích: Xóa Quiz (kèm câu hỏi, bài làm theo FK CASCADE) và dọn media usage của Quiz.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using CourseService.Application.Common.Errors;
using CourseService.Application.Repositories;
using CourseService.Application.Services.Media;
using MediaService.Contracts.Messaging;

namespace CourseService.Application.UseCases.Quizzes.Delete;

public sealed class DeleteQuizHandler(
    IQuizCommandRepository repository,
    ICourseMediaReader mediaReader,
    ICommandSender commandSender)
{
    public async Task HandleAsync(Guid courseId, Guid lessonId, Guid quizId, Guid actorId, CancellationToken cancellationToken)
    {
        if (courseId == Guid.Empty || lessonId == Guid.Empty || quizId == Guid.Empty || actorId == Guid.Empty)
        {
            throw CourseErrors.ValidationFailed([]);
        }

        if (!await repository.LessonExistsAsync(courseId, lessonId, cancellationToken))
        {
            throw CourseErrors.LessonNotFound();
        }

        _ = await repository.GetAsync(lessonId, quizId, cancellationToken) ?? throw CourseErrors.QuizNotFound();
        var usageIds = await mediaReader.GetActiveUsageIdsAsync(CourseMediaUsageScopes.ForQuiz(quizId), cancellationToken);
        if (!await repository.DeleteAsync(lessonId, quizId, cancellationToken))
        {
            throw CourseErrors.QuizNotFound();
        }

        if (usageIds.Count > 0)
        {
            await commandSender.SendAsync(
                ServiceNames.Media,
                new DeleteMediaUsagesByIdsV1(usageIds.Distinct().ToArray(), Guid.NewGuid()),
                cancellationToken);
        }
    }
}
