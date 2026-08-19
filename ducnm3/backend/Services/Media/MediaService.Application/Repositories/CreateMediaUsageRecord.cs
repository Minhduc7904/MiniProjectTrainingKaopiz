// File: backend/Services/Media/MediaService.Application/Repositories/CreateMediaUsageRecord.cs
// Mục đích: Mô tả dữ liệu CreateMediaUsageRecord được repository đọc hoặc ghi giữa Application và Persistence.

using MediaService.Domain.Constants;
using MediaService.Domain.ValueObjects;

namespace MediaService.Application.Repositories;

public sealed record CreateMediaUsageRecord(
    Guid Id,
    Guid MediaId,
    string OwnerService,
    string OwnerType,
    Guid OwnerId,
    string UsageType,
    uint DisplayOrder,
    ActorReference CreatedBy);
