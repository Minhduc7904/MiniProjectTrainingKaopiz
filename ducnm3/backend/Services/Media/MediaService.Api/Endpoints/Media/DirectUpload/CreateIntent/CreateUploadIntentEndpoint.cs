// File: backend/Services/Media/MediaService.Api/Endpoints/Media/DirectUpload/CreateIntent/CreateUploadIntentEndpoint.cs
// Mục đích: Khai báo Minimal API endpoint CreateUploadIntentEndpoint, chuyển HTTP request thành use case và map response.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using MediaService.Api.Contracts.Requests;
using MediaService.Api.Contracts.Responses;
using MediaService.Application.UseCases.Media.DirectUpload.CreateIntent;
using MediaService.Domain.Constants;

namespace MediaService.Api.Endpoints.Media;

public static class CreateUploadIntentEndpoint
{
    public static RouteHandlerBuilder MapCreateUploadIntent(
        this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost(
                ApiRoutes.Media.UploadIntents,
                async (CreateUploadIntentRequest request, HttpContext context,
                    CreateUploadIntentHandler handler, CancellationToken cancellationToken) =>
                {
                    ArgumentNullException.ThrowIfNull(request);
                    var result = await handler.HandleAsync(
                        new CreateUploadIntentCommand(
                            request.OriginalFileName,
                            request.ContentType,
                            request.SizeBytes,
                            request.ChecksumSha256,
                            MediaRequestParser.ReadActor(context.Request)),
                        cancellationToken);
                    context.Response.Headers.Location =
                        ApiRoutes.Media.ResourcePublicPath(result.MediaId);
                    return Results.Json(
                        ApiResponseFactory.Success(
                            new CreateUploadIntentResponse(
                                result.MediaId, result.MediaType, result.Status, result.IsDraft,
                                result.ExpiresAtUtc, result.UploadUrl, result.FormFields),
                            context.TraceIdentifier),
                        statusCode: StatusCodes.Status201Created);
                })
            .WithName("create-media-upload-intent")
            .WithTags(ServiceNames.Media)
            .Accepts<CreateUploadIntentRequest>("application/json")
            .Produces<ApiResponse<CreateUploadIntentResponse>>(StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status413PayloadTooLarge)
            .Produces<ApiErrorResponse>(StatusCodes.Status415UnsupportedMediaType)
            .Produces<ApiErrorResponse>(StatusCodes.Status503ServiceUnavailable);
}
