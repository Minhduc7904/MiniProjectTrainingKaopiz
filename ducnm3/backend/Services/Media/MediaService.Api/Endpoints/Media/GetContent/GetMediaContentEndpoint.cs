// File: backend/Services/Media/MediaService.Api/Endpoints/Media/GetContent/GetMediaContentEndpoint.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

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
                    context.Response.StatusCode = StatusCodes.Status200OK;
                    context.Response.ContentType = result.ContentType;
                    context.Response.ContentLength = result.SizeBytes;
                    context.Response.Headers.ContentDisposition =
                        contentDisposition.ToString();
                    context.Response.Headers.CacheControl = "no-store";
                    await result.CopyToAsync(
                        context.Response.Body,
                        cancellationToken);
                    return Results.Empty;
                })
            .WithName("get-media-content")
            .WithTags(ServiceNames.Media)
            .Produces(StatusCodes.Status200OK, contentType: "application/octet-stream")
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .Produces<ApiErrorResponse>(StatusCodes.Status503ServiceUnavailable);
}
