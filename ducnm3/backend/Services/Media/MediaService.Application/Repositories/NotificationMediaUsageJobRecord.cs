// File: backend/Services/Media/MediaService.Application/Repositories/NotificationMediaUsageJobRecord.cs
// Mục đích: Mang projection job Media Usage giữa Application và Persistence mà không phụ thuộc EF model.

namespace MediaService.Application.Repositories;

public sealed record NotificationMediaUsageJobRecord(
    Guid Id,
    string Status,
    uint? ExpectedUsageCount,
    uint ProcessedUsageCount,
    uint FailedUsageCount,
    DateTime CreatedAtUtc,
    DateTime? StartedAtUtc,
    DateTime? CompletedAtUtc,
    string? LastError);
