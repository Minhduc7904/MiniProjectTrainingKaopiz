namespace MediaService.Application.UseCases.Media.Library;

public sealed record GetMediaLibraryQuery(
    string MediaType,
    string Status,
    string? Cursor,
    int PageSize,
    MediaService.Domain.ValueObjects.ActorReference Actor);
