using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using CourseService.Application.Repositories;
using CourseService.Application.Common.Errors;
using CourseService.Application.Services.Content;
using MediaService.Contracts.Messaging;

namespace CourseService.Application.UseCases.Courses.Update;

public sealed record UpdateCourseCommand(Guid CourseId, bool HasName, string? Name, bool HasDescriptionMarkdown, string? DescriptionMarkdown, bool HasStatus, string? Status, Guid ActorId);

public sealed class UpdateCourseHandler(ICourseCommandRepository repository, ICommandSender commandSender)
{
    public async Task<CourseCommandRecord> HandleAsync(UpdateCourseCommand command, CancellationToken cancellationToken)
    {
        if (command.CourseId == Guid.Empty || command.ActorId == Guid.Empty || (!command.HasName && !command.HasDescriptionMarkdown && !command.HasStatus))
            throw CourseErrors.ValidationFailed([]);
        var current = await repository.GetAsync(command.CourseId, cancellationToken) ?? throw CourseErrors.CourseNotFound();
        var name = command.HasName ? command.Name?.Trim() : current.Name;
        var status = command.HasStatus ? command.Status?.Trim().ToUpperInvariant() : current.Status;
        if (string.IsNullOrWhiteSpace(name) || name.Length is < 3 or > 200 || status is not ("DRAFT" or "PUBLISHED" or "ARCHIVED"))
            throw CourseErrors.ValidationFailed([]);
        var description = command.HasDescriptionMarkdown ? Create.CreateCourseHandler.NormalizeMarkdown(command.DescriptionMarkdown) : current.DescriptionMarkdown;
        var result = await repository.UpdateAsync(command.CourseId, name, description, status, cancellationToken) ?? throw CourseErrors.CourseNotFound();
        if (command.HasDescriptionMarkdown)
        {
            var diff = ContentMediaUsageDiff.Create(current.DescriptionMarkdown, result.DescriptionMarkdown);
            if (diff.Added.Count > 0 || diff.Removed.Count > 0)
                await commandSender.SendAsync(ServiceNames.Media, new SynchronizeMarkdownMediaUsageV1(MarkdownMediaUsageOwnerServices.Course, MarkdownMediaUsageOwnerTypes.CourseDescription, result.Id, command.ActorId, diff.Added, diff.Removed, Guid.NewGuid()), cancellationToken);
        }
        return result;
    }
}
