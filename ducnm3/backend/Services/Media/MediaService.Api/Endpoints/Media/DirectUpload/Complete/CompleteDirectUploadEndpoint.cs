// File: backend/Services/Media/MediaService.Api/Endpoints/Media/DirectUpload/Complete/CompleteDirectUploadEndpoint.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using MediaService.Api.Contracts.Requests;
using MediaService.Api.Contracts.Responses;
using MediaService.Api.Mappers;
using MediaService.Application.UseCases.Media.DirectUpload.Complete;
using MediaService.Domain.ValueObjects;

using MediaService.Domain.Constants;

namespace MediaService.Api.Endpoints.Media;

public static class CompleteDirectUploadEndpoint
{
    public static RouteHandlerBuilder MapCompleteDirectUpload(
        this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost(
                ApiRoutes.Media.UploadCompleteTemplate,
                async (string mediaId, CompleteDirectUploadRequest request,
                    HttpContext context, CompleteDirectUploadHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    ArgumentNullException.ThrowIfNull(request);
                    var result = await handler.HandleAsync(
                        new CompleteDirectUploadCommand(
                            MediaRequestParser.ParseGuid(mediaId, "mediaId"),
                            new ActorReference(
                                request.UploadedByType,
                                MediaRequestParser.ParseGuid(request.UploadedBy, "uploadedBy"))),
                        cancellationToken);
                    return Results.Ok(
                        ApiResponseFactory.Success(
                            MediaResponseMapper.ToResponse(result),
                            context.TraceIdentifier));
                })
            .WithName("complete-direct-media-upload")
            .WithTags(ServiceNames.Media)
            .Accepts<CompleteDirectUploadRequest>("application/json")
            .Produces<ApiResponse<UploadMediaResponse>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .Produces<ApiErrorResponse>(StatusCodes.Status503ServiceUnavailable);
}
