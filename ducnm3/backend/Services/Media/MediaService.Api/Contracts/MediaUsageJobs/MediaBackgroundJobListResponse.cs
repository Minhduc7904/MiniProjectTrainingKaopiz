// File: backend/Services/Media/MediaService.Api/Contracts/MediaUsageJobs/MediaBackgroundJobListResponse.cs
// Mục đích: Khai báo HTTP response an toàn cho danh sách Media background job vận hành.

namespace MediaService.Api.Contracts.MediaUsageJobs;

public sealed record MediaBackgroundJobListResponse(
    Guid Id,
    string JobType,
    string SubjectType,
    Guid SubjectId,
    Guid? CorrelationId,
    string Status,
    uint? ExpectedItemCount,
    uint ProcessedItemCount,
    uint FailedItemCount,
    uint? RemainingItemCount,
    decimal? ProgressPercent,
    uint AttemptCount,
    string? LastError,
    DateTime CreatedAtUtc,
    DateTime? StartedAtUtc,
    DateTime? CompletedAtUtc,
    DateTime UpdatedAtUtc);
