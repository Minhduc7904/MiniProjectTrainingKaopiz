// File: backend/Services/Course/CourseService.Application/UseCases/Quizzes/Create/CreateQuizHandler.cs
// Mục đích: Tạo Quiz kèm câu hỏi và lựa chọn trong Lesson, đồng bộ media nhúng trong Markdown.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using CourseService.Application.Common.Errors;
using CourseService.Application.Repositories;
using CourseService.Application.Services.Content;
using CourseService.Domain.Constants;
using MediaService.Contracts.Messaging;

namespace CourseService.Application.UseCases.Quizzes.Create;

public sealed record CreateQuizCommand(Guid CourseId, Guid LessonId, Guid ActorId, QuizInput Input);

public sealed class CreateQuizHandler(IQuizCommandRepository repository, ICommandSender commandSender)
{
    public async Task<QuizRecord> HandleAsync(CreateQuizCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.CourseId == Guid.Empty || command.LessonId == Guid.Empty || command.ActorId == Guid.Empty)
        {
            throw CourseErrors.ValidationFailed([]);
        }

        var draft = QuizValidation.Normalize(command.Input, QuizStatuses.Draft, existingQuestionCount: 0);
        if (!await repository.LessonExistsAsync(command.CourseId, command.LessonId, cancellationToken))
        {
            throw CourseErrors.LessonNotFound();
        }

        var quiz = await repository.CreateAsync(command.LessonId, draft, cancellationToken);
        var references = LessonMediaReferenceExtractor.Extract(QuizValidation.CombineMarkdown(quiz));
        if (references.Count > 0)
        {
            await commandSender.SendAsync(
                ServiceNames.Media,
                new SynchronizeMarkdownMediaUsageV1(
                    MarkdownMediaUsageOwnerServices.Course,
                    MarkdownMediaUsageOwnerTypes.QuizContent,
                    quiz.Id,
                    command.ActorId,
                    references,
                    [],
                    Guid.NewGuid()),
                cancellationToken);
        }

        return quiz;
    }
}
