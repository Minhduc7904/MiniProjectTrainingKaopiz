using MediaService.Application.Services.Storage;

namespace MediaService.Application.Repositories;

public sealed record MediaLibraryRecord(
    Guid Id,
    string MediaType,
    string ContentType,
    string OriginalFileName,
    long SizeBytes,
    string Status,
    bool IsDraft,
    DateTime? DraftedAtUtc,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc,
    Guid? ThumbnailMediaId,
    string? ThumbnailStatus);
