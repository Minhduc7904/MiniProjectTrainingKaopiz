// File: backend/Services/Media/MediaService.Application/Repositories/MediaUsageOwnerQuery.cs
// Mục đích: Định nghĩa dữ liệu truy vấn cho use case MediaUsageOwnerQuery.

namespace MediaService.Application.Repositories;

public sealed record MediaUsageOwnerQuery(
    string OwnerService,
    string OwnerType,
    string UsageType,
    Guid OwnerId);
