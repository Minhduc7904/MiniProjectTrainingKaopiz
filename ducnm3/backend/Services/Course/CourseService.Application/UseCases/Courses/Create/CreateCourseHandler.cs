using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using CourseService.Application.Repositories;
using CourseService.Application.Common.Errors;
using CourseService.Application.Services.Content;
using MediaService.Contracts.Messaging;

namespace CourseService.Application.UseCases.Courses.Create;

public sealed class CreateCourseHandler(ICourseCommandRepository repository, ICommandSender commandSender)
{
    public async Task<CourseCommandRecord> HandleAsync(string? name, string? descriptionMarkdown, string? status, Guid actorId, CancellationToken cancellationToken)
    {
        var normalizedName = name?.Trim();
        var normalizedStatus = status?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalizedName) || normalizedName.Length is < 3 or > 200 ||
            normalizedStatus is not ("DRAFT" or "PUBLISHED" or "ARCHIVED") || actorId == Guid.Empty)
            throw CourseErrors.ValidationFailed([]);
        var result = await repository.CreateAsync(normalizedName, NormalizeMarkdown(descriptionMarkdown), normalizedStatus, cancellationToken);
        var diff = ContentMediaUsageDiff.Create(null, result.DescriptionMarkdown);
        if (diff.Added.Count > 0)
            await commandSender.SendAsync(ServiceNames.Media, new SynchronizeCourseContentMediaUsageV1(result.Id, CourseContentMediaOwnerTypes.CourseDescription, actorId, diff.Added, []), cancellationToken);
        return result;
    }

    internal static string? NormalizeMarkdown(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
