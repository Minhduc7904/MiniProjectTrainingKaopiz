// File: backend/Services/Media/MediaService.Infrastructure/Persistence/Mappers/MediaUsagePersistenceMapper.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using MediaService.Domain.ValueObjects;
using MediaService.Domain.Entities;
using DatabaseMediaUsage = MediaService.Infrastructure.Persistence.Scaffolded.MediaUsage;

using MediaService.Domain.Constants;

namespace MediaService.Infrastructure.Persistence.Mappers;

public static class MediaUsagePersistenceMapper
{
    public static MediaUsage ToDomain(DatabaseMediaUsage source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return new MediaUsage(
            source.Id,
            source.MediaId,
            source.OwnerService,
            source.OwnerType,
            source.OwnerId,
            source.UsageType,
            source.DisplayOrder,
            new ActorReference(source.CreatedByType, source.CreatedBy),
            source.CreatedAt,
            source.DeletedAt);
    }
}
