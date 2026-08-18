// File: backend/Services/Media/MediaService.Application/Repositories/MediaUsageRecord.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

namespace MediaService.Application.Repositories;

public sealed record MediaUsageRecord(
    Guid Id,
    Guid MediaId,
    string OwnerService,
    string OwnerType,
    Guid OwnerId,
    string UsageType,
    uint DisplayOrder,
    DateTime CreatedAtUtc);
