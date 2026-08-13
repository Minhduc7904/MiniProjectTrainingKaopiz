using MediaService.Application.Abstractions.Storage;
using MediaService.Domain.Actors;

namespace MediaService.Application.Abstractions.Persistence;

public sealed record PendingMediaRecord(
    Guid Id,
    StorageObjectLocation Location,
    string MediaType,
    string ContentType,
    string OriginalFileName,
    long SizeBytes,
    ActorReference UploadedBy);
