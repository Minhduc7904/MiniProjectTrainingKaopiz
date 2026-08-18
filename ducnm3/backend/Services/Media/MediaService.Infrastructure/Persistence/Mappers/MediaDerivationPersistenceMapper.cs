// File: backend/Services/Media/MediaService.Infrastructure/Persistence/Mappers/MediaDerivationPersistenceMapper.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using MediaService.Application.Repositories;
using MediaService.Application.Services.Storage;
using MediaService.Domain.ValueObjects;
using MediaService.Infrastructure.Persistence.Scaffolded;

namespace MediaService.Infrastructure.Persistence.Mappers;

public static class MediaDerivationPersistenceMapper
{
    public static ThumbnailDerivationWork ToThumbnailDerivationWork(
        MediaDerivationJob source) =>
        new(
            source.Id,
            source.SourceMediaId,
            new StorageObjectLocation(
                source.SourceMedia.Bucket,
                source.SourceMedia.ObjectKey),
            source.SourceMedia.MediaType,
            source.SourceMedia.ContentType,
            source.SourceMedia.Status,
            source.DerivativeMediaId,
            new StorageObjectLocation(
                source.DerivativeMedia.Bucket,
                source.DerivativeMedia.ObjectKey),
            source.DerivativeMedia.Status,
            source.Status,
            new ActorReference(
                source.SourceMedia.UploadedByType,
                source.SourceMedia.UploadedBy));
}
