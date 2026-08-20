// File: backend/Services/Media/MediaService.Api/Endpoints/MediaUsages/GetUrls/GetMediaUsageUrlsEndpoint.cs
// Mục đích: Khai báo Minimal API endpoint GetMediaUsageUrlsEndpoint, chuyển HTTP request thành use case và map response.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using MediaService.Api.Contracts.Responses;
using MediaService.Api.Mappers;
using MediaService.Application.UseCases.MediaUsages.GetUrls;

namespace MediaService.Api.Endpoints.Media;

public static class GetMediaUsageUrlsEndpoint
{
    public static RouteHandlerBuilder MapGetMediaUsageUrls(
        this IEndpointRouteBuilder endpoints) =>
        endpoints
            .MapGet(
                ApiRoutes.Media.UsageUrls,
                async (
                    string? ownerService,
                    string? ownerType,
                    string? usageType,
                    string? ownerId,
                    string? ownerIds,
                    HttpContext context,
                    GetMediaUsageUrlsHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var result = await handler.HandleAsync(
                        new GetMediaUsageUrlsQuery(
                            ownerService ?? string.Empty,
                            ownerType ?? string.Empty,
                            usageType ?? string.Empty,
                            ParseOwnerIds(ownerId, ownerIds)),
                        cancellationToken);
                    context.Response.Headers.CacheControl = "no-store";
                    return Results.Json(
                        ApiResponseFactory.Success(
                            result.Select(MediaResponseMapper.ToResponse).ToArray(),
                            context.TraceIdentifier));
                })
            .WithName("get-media-usage-urls")
            .WithTags(ServiceNames.Media)
            .Produces<ApiResponse<IReadOnlyList<MediaUsageUrlResponse>>>(
                StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest);

    private static Guid[] ParseOwnerIds(string? ownerId, string? ownerIds)
    {
        var values = string.IsNullOrWhiteSpace(ownerIds)
            ? [ownerId ?? string.Empty]
            : ownerIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return values.Select(value => MediaRequestParser.ParseGuid(value, "ownerId")).ToArray();
    }
}
