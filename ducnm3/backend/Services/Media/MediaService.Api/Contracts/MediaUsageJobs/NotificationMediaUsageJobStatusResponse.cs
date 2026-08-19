// File: backend/Services/Media/MediaService.Api/Contracts/MediaUsageJobs/NotificationMediaUsageJobStatusResponse.cs
// Mục đích: Khai báo HTTP response tiến độ background job đăng ký Media Usage từ Notification Batch.

namespace MediaService.Api.Contracts.MediaUsageJobs;

public sealed record NotificationMediaUsageJobStatusResponse(
    Guid JobId,
    string Status,
    uint? ExpectedUsageCount,
    uint ProcessedUsageCount,
    uint FailedUsageCount,
    uint? RemainingUsageCount,
    decimal? ProgressPercent,
    DateTime CreatedAtUtc,
    DateTime? StartedAtUtc,
    DateTime? CompletedAtUtc,
    string? ErrorMessage);
