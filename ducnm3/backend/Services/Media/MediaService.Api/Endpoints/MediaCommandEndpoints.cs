using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using MediaService.Application;
using MediaService.Application.Upload;
using MediaService.Application.Usages;
using MediaService.Domain;

namespace MediaService.Api.Endpoints;

public static class MediaCommandEndpoints
{
    public static RouteHandlerBuilder MapUploadMedia(
        this IEndpointRouteBuilder endpoints) =>
        endpoints
            .MapPost(
                "/api/media",
                async (
                    HttpContext context,
                    UploadMediaHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    if (!context.Request.HasFormContentType)
                    {
                        throw InvalidRequest(
                            "Content-Type must be multipart/form-data.");
                    }

                    var form = await context.Request.ReadFormAsync(cancellationToken);
                    var file = form.Files.GetFile("file") ??
                        throw InvalidRequest("file is required.");
                    var mediaType = GetRequiredFormValue(form, "mediaType");
                    var uploadedByType =
                        GetRequiredFormValue(form, "uploadedByType");
                    var uploadedBy = ParseGuid(
                        GetRequiredFormValue(form, "uploadedBy"),
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

                    context.Response.Headers.Location = $"/api/media/{result.Id}";
                    return Results.Json(
                        ApiResponseFactory.Success(
                            result,
                            context.TraceIdentifier),
                        statusCode: StatusCodes.Status201Created);
                })
            .WithName("upload-media")
            .WithTags(ServiceNames.Media)
            .Accepts<UploadMediaForm>("multipart/form-data")
            .Produces<ApiResponse<UploadMediaResult>>(StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status413PayloadTooLarge)
            .Produces<ApiErrorResponse>(StatusCodes.Status415UnsupportedMediaType)
            .Produces<ApiErrorResponse>(StatusCodes.Status503ServiceUnavailable);

    public static RouteHandlerBuilder MapCreateMediaUsage(
        this IEndpointRouteBuilder endpoints) =>
        endpoints
            .MapPost(
                "/api/media/usages",
                async (
                    CreateMediaUsageRequest request,
                    HttpContext context,
                    CreateMediaUsageHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    ArgumentNullException.ThrowIfNull(request);
                    var result = await handler.HandleAsync(
                        new CreateMediaUsageCommand(
                            ParseGuid(request.MediaId, "mediaId"),
                            request.OwnerService,
                            request.OwnerType,
                            ParseGuid(request.OwnerId, "ownerId"),
                            request.UsageType,
                            request.DisplayOrder,
                            new ActorReference(
                                request.CreatedByType,
                                ParseGuid(request.CreatedBy, "createdBy"))),
                        cancellationToken);

                    context.Response.Headers.Location =
                        $"/api/media/usages/{result.Id}";
                    return Results.Json(
                        ApiResponseFactory.Success(
                            result,
                            context.TraceIdentifier),
                        statusCode: StatusCodes.Status201Created);
                })
            .WithName("create-media-usage")
            .WithTags(ServiceNames.Media)
            .Accepts<CreateMediaUsageRequest>("application/json")
            .Produces<ApiResponse<CreateMediaUsageResult>>(
                StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .Produces<ApiErrorResponse>(StatusCodes.Status503ServiceUnavailable);

    private static string GetRequiredFormValue(IFormCollection form, string name)
    {
        var value = form[name].ToString();
        return string.IsNullOrWhiteSpace(value)
            ? throw InvalidRequest($"{name} is required.")
            : value;
    }

    private static Guid ParseGuid(string value, string field) =>
        Guid.TryParse(value, out var parsed) && parsed != Guid.Empty
            ? parsed
            : throw InvalidRequest($"{field} must be a valid UUID.");

    private static MediaApplicationException InvalidRequest(string message) =>
        new(MediaErrorCodes.InvalidMedia, message, 400);
}

public sealed record UploadMediaForm(
    IFormFile File,
    string MediaType,
    string UploadedByType,
    string UploadedBy);

public sealed record CreateMediaUsageRequest(
    string MediaId,
    string OwnerService,
    string OwnerType,
    string OwnerId,
    string UsageType,
    uint DisplayOrder,
    string CreatedByType,
    string CreatedBy);
