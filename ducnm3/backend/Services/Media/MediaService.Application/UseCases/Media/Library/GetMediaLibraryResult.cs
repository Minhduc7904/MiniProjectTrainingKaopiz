using MediaService.Application.Repositories;

namespace MediaService.Application.UseCases.Media.Library;

public sealed record GetMediaLibraryResult(
    IReadOnlyList<MediaLibraryRecord> Items,
    string? NextCursor,
    bool HasMore);
