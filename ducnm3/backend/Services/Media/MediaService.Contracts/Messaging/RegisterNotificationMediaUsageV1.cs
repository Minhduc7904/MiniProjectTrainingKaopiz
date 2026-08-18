// File: backend/Services/Media/MediaService.Contracts/Messaging/RegisterNotificationMediaUsageV1.cs
// Mục đích: Khai báo contract cho Notification Service đăng ký Media Usage, giúp media được đánh dấu đang sử dụng sau khi notification dùng file.

using BuildingBlocks.Messaging.Abstractions;

namespace MediaService.Contracts.Messaging;

public sealed record RegisterNotificationMediaUsageV1(
    Guid NotificationId,
    Guid CreatedBy,
    IReadOnlyList<NotificationMediaUsageReferenceV1> References) : ICommand;

public sealed record RegisterNotificationMediaUsageBatchV1(
    IReadOnlyList<Guid> NotificationIds,
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

public static class NotificationMediaUsageBatchLimits
{
    public const int MaxNotificationIdsPerCommand = 500;
    public const int MaxUsageRowsPerCommand = 1_000;
}
