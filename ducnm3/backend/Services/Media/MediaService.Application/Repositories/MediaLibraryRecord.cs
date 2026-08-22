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
    MediaLibraryThumbnailRecord? Thumbnail);

public sealed record MediaLibraryThumbnailRecord(
    Guid Id,
    string MediaStatus,
    string? BackgroundJobStatus,
    string ContentType,
    long SizeBytes,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc);
