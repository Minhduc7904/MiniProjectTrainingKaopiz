// File: backend/Services/Media/MediaService.Domain/Entities/MediaUsage.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using MediaService.Domain.ValueObjects;

using MediaService.Domain.Constants;

namespace MediaService.Domain.Entities;

public sealed record MediaUsage(
    Guid Id,
    Guid MediaId,
    string OwnerService,
    string OwnerType,
    Guid OwnerId,
    string UsageType,
    uint DisplayOrder,
    ActorReference CreatedBy,
    DateTime CreatedAtUtc,
    DateTime? DeletedAtUtc = null);
