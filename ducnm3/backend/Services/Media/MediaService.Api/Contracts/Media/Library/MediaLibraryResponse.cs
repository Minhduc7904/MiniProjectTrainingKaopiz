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
    string ContentUrl,
    string ThumbnailStatus,
    string? ThumbnailUrl);

public sealed record MediaLibraryPageResponse(
    IReadOnlyList<MediaLibraryResponse> Items,
    string? NextCursor,
    bool HasMore);
