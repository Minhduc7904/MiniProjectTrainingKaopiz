// File: backend/Services/Media/MediaService.Application/Repositories/MediaUsageRecord.cs
// Mục đích: Mô tả dữ liệu MediaUsageRecord được repository đọc hoặc ghi giữa Application và Persistence.

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
