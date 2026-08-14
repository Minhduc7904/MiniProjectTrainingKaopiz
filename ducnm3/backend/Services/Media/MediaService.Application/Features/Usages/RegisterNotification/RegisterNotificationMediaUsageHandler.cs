using MediaService.Application.Abstractions.Persistence;
using MediaService.Contracts.Messaging;
using MediaService.Domain.Usages;

namespace MediaService.Application.Features.Usages.RegisterNotification;

public sealed class RegisterNotificationMediaUsageHandler(IMediaRepository mediaRepository)
{
    public Task HandleAsync(
        RegisterNotificationMediaUsageV1 command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.NotificationId == Guid.Empty || command.CreatedBy == Guid.Empty ||
            command.References.Count == 0 ||
            command.References.Any(reference =>
                reference.MediaId == Guid.Empty ||
                reference.UsageType is not (NotificationMediaUsageTypes.Embed or NotificationMediaUsageTypes.Attachment)))
        {
            throw MediaErrors.InvalidMedia("Notification media usage command is invalid.");
        }

        return mediaRepository.EnsureNotificationBodyUsagesAsync(
            command.NotificationId,
            command.CreatedBy,
            command.References,
            cancellationToken);
    }
}
