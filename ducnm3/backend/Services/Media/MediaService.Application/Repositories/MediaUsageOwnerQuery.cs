// File: backend/Services/Media/MediaService.Application/Repositories/MediaUsageOwnerQuery.cs
// Mục đích: Định nghĩa dữ liệu truy vấn cho use case MediaUsageOwnerQuery.

namespace MediaService.Application.Repositories;

public sealed record MediaUsageOwnerQuery(
    string OwnerService,
    string OwnerType,
    string UsageType,
    IReadOnlyList<Guid> OwnerIds)
{
    public MediaUsageOwnerQuery(
        string ownerService,
        string ownerType,
        string usageType,
        Guid ownerId)
        : this(ownerService, ownerType, usageType, [ownerId])
    {
    }
}
