using BuildingBlocks.Messaging.Abstractions;

namespace MediaService.Contracts.Messaging;

public sealed record RegisterNotificationMediaUsageV1(
    Guid NotificationId,
    Guid CreatedBy,
    IReadOnlyList<NotificationMediaUsageReferenceV1> References) : ICommand;

public sealed record NotificationMediaUsageReferenceV1(
    Guid MediaId,
    string UsageType,
    uint DisplayOrder);

public static class NotificationMediaUsageTypes
{
    public const string Attachment = "ATTACHMENT";
    public const string Embed = "EMBED";
}
