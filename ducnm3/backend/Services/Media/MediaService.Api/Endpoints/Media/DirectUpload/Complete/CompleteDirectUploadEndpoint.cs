// File: backend/Services/Media/MediaService.Api/Endpoints/Media/DirectUpload/Complete/CompleteDirectUploadEndpoint.cs
// Mục đích: Khai báo Minimal API endpoint CompleteDirectUploadEndpoint, chuyển HTTP request thành use case và map response.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Actors;
using BuildingBlocks.Presentation.Extensions;
using MediaService.Api.Contracts.Responses;
using MediaService.Api.Mappers;
using MediaService.Application.UseCases.Media.DirectUpload.Complete;
using MediaService.Domain.Constants;

namespace MediaService.Api.Endpoints.Media;

public static class CompleteDirectUploadEndpoint
{
    public static RouteHandlerBuilder MapCompleteDirectUpload(
        this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost(
                ApiRoutes.Media.UploadCompleteTemplate,
                async (string mediaId,
                    HttpContext context, CompleteDirectUploadHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var result = await handler.HandleAsync(
                        new CompleteDirectUploadCommand(
                            MediaRequestParser.ParseGuid(mediaId, "mediaId"),
                            MediaRequestParser.ReadActor(context)),
                        cancellationToken);
                    return Results.Ok(
                        ApiResponseFactory.Success(
                            MediaResponseMapper.ToResponse(result),
                            context.TraceIdentifier));
                })
            .WithName("complete-direct-media-upload")
            .WithTags(ServiceNames.Media)
            .Produces<ApiResponse<UploadMediaResponse>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .Produces<ApiErrorResponse>(StatusCodes.Status503ServiceUnavailable)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .RequireActor(ActorAccess.Any);
}
