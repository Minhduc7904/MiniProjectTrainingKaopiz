// File: backend/Services/Media/MediaService.Api/Endpoints/Media/Upload/UploadMediaEndpoint.cs
// Mục đích: Khai báo Minimal API endpoint UploadMediaEndpoint, chuyển HTTP request thành use case và map response.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using MediaService.Api.Contracts.Requests;
using MediaService.Api.Contracts.Responses;
using MediaService.Api.Mappers;
using MediaService.Application;
using MediaService.Application.UseCases.Media.Upload;
using MediaService.Domain.ValueObjects;

using MediaService.Domain.Constants;

using MediaService.Application.Common.Errors;

namespace MediaService.Api.Endpoints.Media;

public static class UploadMediaEndpoint
{
    public static RouteHandlerBuilder MapUploadMedia(
        this IEndpointRouteBuilder endpoints) =>
        endpoints
            .MapPost(
                ApiRoutes.Media.Upload,
                async (
                    HttpContext context,
                    UploadMediaHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var form = await MediaRequestParser.ReadMultipartFormAsync(
                        context.Request,
                        cancellationToken);
                    var file = form.Files.GetFile("file") ??
                        throw MediaErrors.InvalidMedia("file is required.");
                    var mediaType = MediaRequestParser.GetRequiredFormValue(
                        form,
                        "mediaType");
                    var uploadedByType =
                        MediaRequestParser.GetRequiredFormValue(
                            form,
                            "uploadedByType");
                    var uploadedBy = MediaRequestParser.ParseGuid(
                        MediaRequestParser.GetRequiredFormValue(
                            form,
                            "uploadedBy"),
                        "uploadedBy");

                    await using var stream = file.OpenReadStream();
                    var result = await handler.HandleAsync(
                        new UploadMediaCommand(
                            mediaType,
                            file.ContentType,
                            file.FileName,
                            stream,
                            file.Length,
                            new ActorReference(uploadedByType, uploadedBy)),
                        cancellationToken);
                    var response = MediaResponseMapper.ToResponse(result);
                    context.Response.Headers.Location =
                        ApiRoutes.BuildPublicPath(
                            GatewayRoutePrefixes.Media,
                            $"{ApiRoutes.Media.Upload}/{result.Id:D}");

                    return Results.Json(
                        ApiResponseFactory.Success(
                            response,
                            context.TraceIdentifier),
                        statusCode: StatusCodes.Status201Created);
                })
            .WithName("upload-media")
            .WithTags(ServiceNames.Media)
            .Accepts<UploadMediaForm>("multipart/form-data")
            .Produces<ApiResponse<UploadMediaResponse>>(
                StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status413PayloadTooLarge)
            .Produces<ApiErrorResponse>(StatusCodes.Status415UnsupportedMediaType)
            .Produces<ApiErrorResponse>(StatusCodes.Status503ServiceUnavailable);
}
