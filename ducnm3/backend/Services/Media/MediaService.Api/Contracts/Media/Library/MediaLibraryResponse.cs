namespace MediaService.Api.Contracts.Responses;

public sealed record MediaLibraryResponse(
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
    string ContentUrl,
    string ThumbnailStatus,
    string? ThumbnailUrl,
    MediaLibraryThumbnailResponse? Thumbnail);

public sealed record MediaLibraryThumbnailResponse(
    Guid Id,
    string Status,
    string ContentType,
    long SizeBytes,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc,
    string? ContentUrl);

public sealed record MediaLibraryPageResponse(
    IReadOnlyList<MediaLibraryResponse> Items,
    string? NextCursor,
    bool HasMore);
