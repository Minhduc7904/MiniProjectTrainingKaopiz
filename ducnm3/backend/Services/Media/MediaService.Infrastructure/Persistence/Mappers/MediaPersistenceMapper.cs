// File: backend/Services/Media/MediaService.Infrastructure/Persistence/Mappers/MediaPersistenceMapper.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using MediaService.Domain.ValueObjects;
using MediaService.Domain.Entities;
using DatabaseMediaObject = MediaService.Infrastructure.Persistence.Scaffolded.MediaObject;

using MediaService.Domain.Constants;

namespace MediaService.Infrastructure.Persistence.Mappers;

public static class MediaPersistenceMapper
{
    public static Media ToDomain(DatabaseMediaObject source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return new Media(
            source.Id,
            source.Bucket,
            source.ObjectKey,
            source.MediaType,
            source.ContentType,
            source.OriginalFileName,
            checked((long)source.SizeBytes),
            source.Status,
            source.CreatedAt,
            source.DeletedAt,
            source.SourceMediaId,
            source.DerivationType,
            source.IsDraft,
            source.DraftedAt,
            source.ChecksumSha256,
            new ActorReference(source.UploadedByType, source.UploadedBy));
    }
}
