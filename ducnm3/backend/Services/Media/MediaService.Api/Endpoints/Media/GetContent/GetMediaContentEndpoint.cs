// File: backend/Services/Media/MediaService.Api/Endpoints/Media/GetContent/GetMediaContentEndpoint.cs
// Mục đích: Khai báo Minimal API endpoint GetMediaContentEndpoint, chuyển HTTP request thành use case và map response.

using System.Net.Http.Headers;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using MediaService.Application.UseCases.Media.GetContent;

namespace MediaService.Api.Endpoints.Media;

public static class GetMediaContentEndpoint
{
    public static RouteHandlerBuilder MapGetMediaContent(
        this IEndpointRouteBuilder endpoints) =>
        endpoints
            .MapGet(
                ApiRoutes.Media.ContentTemplate,
                async (
                    string mediaId,
                    HttpContext context,
                    GetMediaContentHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var result = await handler.HandleAsync(
                        new GetMediaContentQuery(
                            MediaRequestParser.ParseGuid(mediaId, "mediaId")),
                        cancellationToken);
                    var contentDisposition = new ContentDispositionHeaderValue(
                        "inline")
                    {
                        FileNameStar = result.OriginalFileName,
                    };
                    context.Response.ContentType = result.ContentType;
                    context.Response.Headers.ContentDisposition =
                        contentDisposition.ToString();
                    context.Response.Headers.CacheControl = "no-store";
                    context.Response.Headers.Append("Accept-Ranges", "bytes");

                    var range = ParseSingleByteRange(
                        context.Request.Headers.Range.ToString(),
                        result.SizeBytes);
                    if (range.IsInvalid)
                    {
                        context.Response.Headers.Append(
                            "Content-Range",
                            $"bytes */{result.SizeBytes}");
                        return Results.StatusCode(
                            StatusCodes.Status416RangeNotSatisfiable);
                    }

                    if (range.Start is null || range.Length is null)
                    {
                        context.Response.StatusCode = StatusCodes.Status200OK;
                        context.Response.ContentLength = result.SizeBytes;
                        await result.CopyToAsync(
                            context.Response.Body,
                            cancellationToken);
                        return Results.Empty;
                    }

                    context.Response.StatusCode = StatusCodes.Status206PartialContent;
                    context.Response.ContentLength = range.Length.Value;
                    context.Response.Headers.Append(
                        "Content-Range",
                        $"bytes {range.Start.Value}-{range.Start.Value + range.Length.Value - 1}/{result.SizeBytes}");
                    await result.CopyToAsync(
                        context.Response.Body,
                        range.Start.Value,
                        range.Length.Value,
                        cancellationToken);
                    return Results.Empty;
                })
            .WithName("get-media-content")
            .WithTags(ServiceNames.Media)
            .Produces(StatusCodes.Status200OK, contentType: "application/octet-stream")
            .Produces(StatusCodes.Status206PartialContent, contentType: "application/octet-stream")
            .Produces(StatusCodes.Status416RangeNotSatisfiable)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .Produces<ApiErrorResponse>(StatusCodes.Status503ServiceUnavailable);

    private static ByteRange ParseSingleByteRange(string value, long totalLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ByteRange.Full;
        }

        if (totalLength <= 0 || !value.StartsWith("bytes=", StringComparison.OrdinalIgnoreCase))
        {
            return ByteRange.Invalid;
        }

        var specification = value[6..];
        if (specification.Contains(',', StringComparison.Ordinal))
        {
            return ByteRange.Invalid;
        }

        var parts = specification.Split('-', StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || (parts[0].Length == 0 && parts[1].Length == 0))
        {
            return ByteRange.Invalid;
        }

        if (parts[0].Length == 0)
        {
            if (!long.TryParse(parts[1], out var suffixLength) || suffixLength <= 0)
            {
                return ByteRange.Invalid;
            }

            var length = Math.Min(suffixLength, totalLength);
            return new ByteRange(totalLength - length, length, false);
        }

        if (!long.TryParse(parts[0], out var start) || start < 0 || start >= totalLength)
        {
            return ByteRange.Invalid;
        }

        var end = totalLength - 1;
        if (parts[1].Length > 0 &&
            (!long.TryParse(parts[1], out end) || end < start))
        {
            return ByteRange.Invalid;
        }

        end = Math.Min(end, totalLength - 1);
        return new ByteRange(start, end - start + 1, false);
    }

    private readonly record struct ByteRange(long? Start, long? Length, bool IsInvalid)
    {
        public static ByteRange Full => new(null, null, false);

        public static ByteRange Invalid => new(null, null, true);
    }
}
