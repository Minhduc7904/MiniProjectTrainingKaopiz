// File: backend/Services/Course/CourseService.Application/UseCases/Quizzes/Update/UpdateQuizHandler.cs
// Mục đích: Cập nhật Quiz; chỉ đổi cây câu hỏi khi Quiz chưa có bài làm, đồng bộ diff media nhúng.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using CourseService.Application.Common.Errors;
using CourseService.Application.Repositories;
using CourseService.Application.Services.Content;
using MediaService.Contracts.Messaging;

namespace CourseService.Application.UseCases.Quizzes.Update;

public sealed record UpdateQuizCommand(Guid CourseId, Guid LessonId, Guid QuizId, Guid ActorId, QuizInput Input);

public sealed class UpdateQuizHandler(IQuizCommandRepository repository, ICommandSender commandSender)
{
    public async Task<QuizRecord> HandleAsync(UpdateQuizCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.CourseId == Guid.Empty || command.LessonId == Guid.Empty || command.QuizId == Guid.Empty || command.ActorId == Guid.Empty)
        {
            throw CourseErrors.ValidationFailed([]);
        }

        if (!await repository.LessonExistsAsync(command.CourseId, command.LessonId, cancellationToken))
        {
            throw CourseErrors.LessonNotFound();
        }

        var current = await repository.GetAsync(command.LessonId, command.QuizId, cancellationToken)
            ?? throw CourseErrors.QuizNotFound();
        var draft = QuizValidation.Normalize(command.Input, defaultStatus: null, current.Questions.Count);
        if (draft.Questions is not null && await repository.HasAttemptsAsync(current.Id, cancellationToken))
        {
            throw CourseErrors.QuizHasAttempts();
        }

        var updated = await repository.UpdateAsync(command.LessonId, command.QuizId, draft, cancellationToken)
            ?? throw CourseErrors.QuizNotFound();
        var diff = ContentMediaUsageDiff.Create(QuizValidation.CombineMarkdown(current), QuizValidation.CombineMarkdown(updated));
        if (diff.Added.Count > 0 || diff.Removed.Count > 0)
        {
            await commandSender.SendAsync(
                ServiceNames.Media,
                new SynchronizeMarkdownMediaUsageV1(
                    MarkdownMediaUsageOwnerServices.Course,
                    MarkdownMediaUsageOwnerTypes.QuizContent,
                    updated.Id,
                    command.ActorId,
                    diff.Added,
                    diff.Removed,
                    Guid.NewGuid()),
                cancellationToken);
        }

        return updated;
    }
}
