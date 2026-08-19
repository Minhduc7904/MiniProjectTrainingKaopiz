// File: backend/Services/Media/MediaService.Domain/Constants/NotificationMediaUsageJobStatuses.cs
// Mục đích: Khai báo trạng thái vòng đời job đăng ký Media Usage từ Notification Batch.

namespace MediaService.Domain.Constants;

public static class NotificationMediaUsageJobStatuses
{
    public const string Pending = "PENDING";
    public const string Processing = "PROCESSING";
    public const string Completed = "COMPLETED";
    public const string PartialFailed = "PARTIAL_FAILED";
    public const string Failed = "FAILED";
}
