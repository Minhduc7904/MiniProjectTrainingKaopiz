using MediaService.Application.Abstractions.Storage;

namespace MediaService.Application.Abstractions.Persistence;

public sealed record MediaRecord(
    Guid Id,
    StorageObjectLocation Location,
    string MediaType,
    string ContentType,
    string OriginalFileName,
    long SizeBytes,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? DeletedAtUtc,
    Guid? SourceMediaId = null,
    string? DerivationType = null);
