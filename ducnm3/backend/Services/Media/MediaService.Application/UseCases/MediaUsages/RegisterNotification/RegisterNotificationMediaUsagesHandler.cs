// File: backend/Services/Media/MediaService.Application/UseCases/MediaUsages/RegisterNotification/RegisterNotificationMediaUsagesHandler.cs
// Mục đích: Điều phối use case RegisterNotificationMediaUsagesHandler: validate input, gọi port và trả kết quả nghiệp vụ.

using MediaService.Application.Common.Errors;
using MediaService.Application.Repositories;
using MediaService.Contracts.Messaging;
using MediaService.Domain.Constants;
using MediaService.Domain.ValueObjects;

namespace MediaService.Application.UseCases.MediaUsages.RegisterNotification;

public sealed class RegisterNotificationMediaUsagesHandler(
    IMediaUsageRepository mediaUsageRepository,
    INotificationMediaUsageJobRepository jobRepository)
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

    public async Task HandleAsync(
        RegisterNotificationMediaUsageBatchV1 command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.JobId == Guid.Empty)
        {
            throw MediaErrors.InvalidMedia("Notification media usage jobId is invalid.");
        }
        await HandleAsync(
            command.NotificationIds,
            command.CreatedBy,
            command.References,
            cancellationToken);
        var usageCount = checked((uint)(
            command.NotificationIds.Distinct().Count() * command.References.Count));
        await jobRepository.RecordSuccessAsync(command.JobId, usageCount, cancellationToken);
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
        return mediaUsageRepository.EnsureMediaUsagesAsync(usages, cancellationToken);
    }
}
