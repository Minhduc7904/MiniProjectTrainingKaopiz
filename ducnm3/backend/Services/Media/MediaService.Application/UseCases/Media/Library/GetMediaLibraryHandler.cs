using System.Text;
using System.Text.Json;
using MediaService.Application.Common.Errors;
using MediaService.Application.Repositories;
using MediaService.Domain.Constants;
using MediaService.Domain.ValueObjects;

namespace MediaService.Application.UseCases.Media.Library;

public sealed class GetMediaLibraryHandler(IMediaRepository repository)
{
    public async Task<GetMediaLibraryResult> HandleAsync(
        GetMediaLibraryQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.PageSize is < 1 or > 50)
            throw MediaErrors.InvalidMedia("pageSize must be between 1 and 50.");

        var mediaType = string.IsNullOrWhiteSpace(query.MediaType)
            ? null
            : query.MediaType.Trim().ToUpperInvariant();
        if (mediaType is not null && !MediaTypes.All.Contains(mediaType))
            throw MediaErrors.InvalidMedia("mediaType is not supported.");

        var cursor = DecodeCursor(query.Cursor);
        var page = await repository.ListByActorAsync(
            query.Actor, mediaType, cursor, query.PageSize + 1, cancellationToken);
        var hasMore = page.Count > query.PageSize;
        var items = page.Take(query.PageSize).ToArray();
        var nextCursor = hasMore && items.Length > 0
            ? EncodeCursor(items[^1].CreatedAtUtc, items[^1].Id)
            : null;
        return new GetMediaLibraryResult(items, nextCursor, hasMore);
    }

    private static (DateTime CreatedAtUtc, Guid Id)? DecodeCursor(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(value));
            var cursor = JsonSerializer.Deserialize<Cursor>(json);
            return cursor is not null && cursor.Id != Guid.Empty
                ? (cursor.CreatedAtUtc, cursor.Id)
                : throw new FormatException();
        }
        catch (Exception) when (value.Length > 0)
        {
            throw MediaErrors.InvalidMedia("cursor is invalid.");
        }
    }

    public static string EncodeCursor(DateTime createdAtUtc, Guid id) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(
            new Cursor(createdAtUtc, id))));

    private sealed record Cursor(DateTime CreatedAtUtc, Guid Id);
}
