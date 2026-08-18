// File: backend/Services/Media/MediaService.Api/Endpoints/MediaUsages/Create/CreateMediaUsageEndpoint.cs
// Mục đích: Khai báo Minimal API endpoint CreateMediaUsageEndpoint, chuyển HTTP request thành use case và map response.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using MediaService.Api.Contracts.Requests;
using MediaService.Api.Contracts.Responses;
using MediaService.Api.Mappers;
using MediaService.Application.UseCases.MediaUsages.Create;
using MediaService.Domain.ValueObjects;

using MediaService.Domain.Constants;

namespace MediaService.Api.Endpoints.Media;

public static class CreateMediaUsageEndpoint
{
    public static RouteHandlerBuilder MapCreateMediaUsage(
        this IEndpointRouteBuilder endpoints) =>
        endpoints
            .MapPost(
                ApiRoutes.Media.Usages,
                async (
                    CreateMediaUsageRequest request,
                    HttpContext context,
                    CreateMediaUsageHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    ArgumentNullException.ThrowIfNull(request);
                    var result = await handler.HandleAsync(
                        new CreateMediaUsageCommand(
                            MediaRequestParser.ParseGuid(
                                request.MediaId,
                                "mediaId"),
                            request.OwnerService,
                            request.OwnerType,
                            MediaRequestParser.ParseGuid(
                                request.OwnerId,
                                "ownerId"),
                            request.UsageType,
                            request.DisplayOrder,
                            new ActorReference(
                                request.CreatedByType,
                                MediaRequestParser.ParseGuid(
                                    request.CreatedBy,
                                    "createdBy"))),
                        cancellationToken);
                    var response = MediaResponseMapper.ToResponse(result);
                    context.Response.Headers.Location =
                        ApiRoutes.BuildPublicPath(
                            GatewayRoutePrefixes.Media,
                            $"{ApiRoutes.Media.Usages}/{result.Id:D}");

                    return Results.Json(
                        ApiResponseFactory.Success(
                            response,
                            context.TraceIdentifier),
                        statusCode: StatusCodes.Status201Created);
                })
            .WithName("create-media-usage")
            .WithTags(ServiceNames.Media)
            .Accepts<CreateMediaUsageRequest>("application/json")
            .Produces<ApiResponse<CreateMediaUsageResponse>>(
                StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .Produces<ApiErrorResponse>(StatusCodes.Status503ServiceUnavailable);
}
