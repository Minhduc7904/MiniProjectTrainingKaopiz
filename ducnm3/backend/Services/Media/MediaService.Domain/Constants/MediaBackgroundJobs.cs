// File: backend/Services/Media/MediaService.Domain/Constants/MediaBackgroundJobs.cs
// Mục đích: Khai báo loại và trạng thái chuẩn cho mọi công việc bất đồng bộ của Media Worker.

namespace MediaService.Domain.Constants;

public static class MediaBackgroundJobTypes
{
    public const string ThumbnailDerivation = "THUMBNAIL_DERIVATION";
    public const string MarkdownUsageSync = "MARKDOWN_USAGE_SYNC";
    public const string MediaUsageDelete = "MEDIA_USAGE_DELETE";
    public const string NotificationUsage = "NOTIFICATION_USAGE";
    public static IReadOnlySet<string> All { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        ThumbnailDerivation, MarkdownUsageSync, MediaUsageDelete, NotificationUsage,
    };
}

public static class MediaBackgroundJobStatuses
{
    public const string Queued = "QUEUED";
    public const string Processing = "PROCESSING";
    public const string Completed = "COMPLETED";
    public const string PartialFailed = "PARTIAL_FAILED";
    public const string Failed = "FAILED";
    public static IReadOnlySet<string> All { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        Queued, Processing, Completed, PartialFailed, Failed,
    };
}
