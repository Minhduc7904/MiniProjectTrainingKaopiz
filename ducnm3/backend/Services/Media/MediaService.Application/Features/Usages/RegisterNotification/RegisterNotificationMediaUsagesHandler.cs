using MediaService.Application.Abstractions.Persistence;
using MediaService.Contracts.Messaging;
using MediaService.Domain.Actors;
using MediaService.Domain.Usages;

namespace MediaService.Application.Features.Usages.RegisterNotification;

public sealed class RegisterNotificationMediaUsagesHandler(IMediaRepository mediaRepository)
{
    public Task HandleAsync(
        RegisterNotificationMediaUsageV1 command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        return HandleAsync(
            [command.NotificationId],
            command.CreatedBy,
            command.References,
            cancellationToken);
    }

    public Task HandleAsync(
        RegisterNotificationMediaUsageBatchV1 command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        return HandleAsync(
            command.NotificationIds,
            command.CreatedBy,
            command.References,
            cancellationToken);
    }

    private Task HandleAsync(
        IReadOnlyList<Guid> notificationIds,
        Guid createdBy,
        IReadOnlyList<NotificationMediaUsageReferenceV1> references,
        CancellationToken cancellationToken)
    {
        var distinctNotificationIds = notificationIds.Distinct().ToArray();
        if (createdBy == Guid.Empty ||
            distinctNotificationIds.Length == 0 ||
            distinctNotificationIds.Length > NotificationMediaUsageBatchLimits.MaxNotificationIdsPerCommand ||
            distinctNotificationIds.Any(id => id == Guid.Empty) ||
            references.Count == 0 ||
            references.Any(reference =>
                reference.MediaId == Guid.Empty ||
                reference.UsageType is not (NotificationMediaUsageTypes.Embed or NotificationMediaUsageTypes.Attachment)) ||
            (long)distinctNotificationIds.Length * references.Count >
                NotificationMediaUsageBatchLimits.MaxUsageRowsPerCommand)
        {
            throw MediaErrors.InvalidMedia("Notification media usage command is invalid.");
        }

        var actor = new ActorReference(ActorTypes.Admin, createdBy);
        var usages = distinctNotificationIds
            .SelectMany(notificationId => references.Select(reference =>
                new CreateMediaUsageRecord(
                    Guid.NewGuid(),
                    reference.MediaId,
                    MediaOwnerServices.Notification,
                    MediaOwnerTypes.NotificationBody,
                    notificationId,
                    reference.UsageType,
                    reference.DisplayOrder,
                    actor)))
            .ToArray();
        return mediaRepository.EnsureMediaUsagesAsync(usages, cancellationToken);
    }
}
