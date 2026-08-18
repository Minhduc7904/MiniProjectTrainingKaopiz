// File: backend/Services/Media/MediaService.Application/Repositories/MediaUsageOwnerQuery.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

namespace MediaService.Application.Repositories;

public sealed record MediaUsageOwnerQuery(
    string OwnerService,
    string OwnerType,
    string UsageType,
    Guid OwnerId);
