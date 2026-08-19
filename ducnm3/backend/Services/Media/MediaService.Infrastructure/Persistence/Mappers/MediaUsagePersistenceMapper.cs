// File: backend/Services/Media/MediaService.Infrastructure/Persistence/Mappers/MediaUsagePersistenceMapper.cs
// Mục đích: Chuyển đổi giữa persistence model EF Core và domain/application model cho MediaUsagePersistenceMapper.

using MediaService.Domain.Constants;
using MediaService.Domain.Entities;
using MediaService.Domain.ValueObjects;
using DatabaseMediaUsage = MediaService.Infrastructure.Persistence.Scaffolded.MediaUsage;

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
