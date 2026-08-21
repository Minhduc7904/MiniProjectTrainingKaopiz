// File: backend/Services/Media/MediaService.Application/Repositories/MediaBackgroundJobListRecord.cs
// Mục đích: Khai báo projection đọc-only của Media background job để hiển thị danh sách vận hành.

namespace MediaService.Application.Repositories;

public sealed record MediaBackgroundJobListRecord(
    Guid Id,
    string JobType,
    string SubjectType,
    Guid SubjectId,
    Guid? CorrelationId,
    string Status,
    uint? ExpectedItemCount,
    uint ProcessedItemCount,
    uint FailedItemCount,
    uint AttemptCount,
    string? LastError,
    DateTime CreatedAtUtc,
    DateTime? StartedAtUtc,
    DateTime? CompletedAtUtc,
    DateTime UpdatedAtUtc);
