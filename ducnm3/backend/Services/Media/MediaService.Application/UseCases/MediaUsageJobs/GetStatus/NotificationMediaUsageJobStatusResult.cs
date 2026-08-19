// File: backend/Services/Media/MediaService.Application/UseCases/MediaUsageJobs/GetStatus/NotificationMediaUsageJobStatusResult.cs
// Mục đích: Định nghĩa response model của status job đăng ký Media Usage từ Markdown.

namespace MediaService.Application.UseCases.MediaUsageJobs.GetStatus;

public sealed record NotificationMediaUsageJobStatusResult(
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
