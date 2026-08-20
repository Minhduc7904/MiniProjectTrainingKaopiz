using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using CourseService.Application.Repositories;
using CourseService.Application.Common.Errors;
using CourseService.Application.Services.Content;
using MediaService.Contracts.Messaging;

namespace CourseService.Application.UseCases.Lessons.Update;

public sealed record UpdateLessonCommand(Guid CourseId, Guid LessonId, bool HasTitle, string? Title, bool HasContentMarkdown, string? ContentMarkdown, Guid ActorId);

public sealed class UpdateLessonHandler(ILessonCommandRepository repository, ICommandSender commandSender)
{
    public async Task<LessonCreateRecord> HandleAsync(UpdateLessonCommand command, CancellationToken cancellationToken)
    {
        if (command.CourseId == Guid.Empty || command.LessonId == Guid.Empty || command.ActorId == Guid.Empty || (!command.HasTitle && !command.HasContentMarkdown))
            throw CourseErrors.ValidationFailed([]);
        var current = await repository.GetAsync(command.CourseId, command.LessonId, cancellationToken) ?? throw CourseErrors.LessonNotFound();
        var title = command.HasTitle ? command.Title?.Trim() : current.Title;
        if (string.IsNullOrWhiteSpace(title) || title.Length > 200)
            throw CourseErrors.ValidationFailed([]);
        var content = command.HasContentMarkdown ? Create.CreateLessonHandler.NormalizeMarkdown(command.ContentMarkdown) : current.ContentMarkdown;
        var result = await repository.UpdateAsync(command.CourseId, command.LessonId, title, content, cancellationToken) ?? throw CourseErrors.LessonNotFound();
        if (command.HasContentMarkdown)
        {
            var diff = ContentMediaUsageDiff.Create(current.ContentMarkdown, result.ContentMarkdown);
            if (diff.Added.Count > 0 || diff.Removed.Count > 0)
                await commandSender.SendAsync(ServiceNames.Media, new SynchronizeCourseContentMediaUsageV1(result.Id, CourseContentMediaOwnerTypes.LessonContent, command.ActorId, diff.Added, diff.Removed), cancellationToken);
        }
        return result;
    }
}
