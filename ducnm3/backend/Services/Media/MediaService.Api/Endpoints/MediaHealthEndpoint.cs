using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Contracts.Health;
using BuildingBlocks.Presentation.Api;
using MediaService.Application.Storage;

namespace MediaService.Api.Endpoints;

public static class MediaHealthEndpoint
{
    public static RouteHandlerBuilder MapMediaHealthEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapGet(
                ApiPaths.Health,
                async (
                    HttpContext context,
                    IDatabaseHealthProbe databaseHealthProbe,
                    IStorageHealthProbe storageHealthProbe,
                    IMessagingHealthProbe messagingHealthProbe,
                    CancellationToken cancellationToken) =>
                {
                    var databaseTask = databaseHealthProbe.CheckAsync(cancellationToken);
                    var storageTask = storageHealthProbe.CheckAsync(cancellationToken);
                    var messagingTask = messagingHealthProbe.CheckAsync(cancellationToken);
                    await Task.WhenAll(databaseTask, storageTask, messagingTask);

                    var databaseHealthy = databaseTask.Result.IsHealthy;
                    var storageHealthy = storageTask.Result.IsHealthy;
                    var messagingHealthy = messagingTask.Result.IsHealthy;

                    if (!databaseHealthy || !storageHealthy || !messagingHealthy)
                    {
                        var (code, message) = SelectDependencyError(
                            databaseHealthy,
                            storageHealthy,
                            messagingHealthy);
                        return Results.Json(
                            ApiResponseFactory.Error(code, message, context.TraceIdentifier),
                            statusCode: StatusCodes.Status503ServiceUnavailable);
                    }

                    return Results.Json(
                        ApiResponseFactory.Success(
                            new MediaServiceHealthResponse(
                                ServiceNames.Media,
                                HealthStatusValues.Healthy,
                                new DatabaseHealthResponse(HealthStatusValues.Healthy),
                                new StorageHealthResponse(HealthStatusValues.Healthy),
                                new MessagingHealthResponse(HealthStatusValues.Healthy)),
                            context.TraceIdentifier));
                })
            .WithName($"{ServiceNames.Media}-health")
            .WithTags(ServiceNames.Media)
            .Produces<ApiResponse<MediaServiceHealthResponse>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status503ServiceUnavailable);
    }

    private static (string Code, string Message) SelectDependencyError(
        bool databaseHealthy,
        bool storageHealthy,
        bool messagingHealthy)
    {
        if ((!databaseHealthy && !storageHealthy) ||
            (!databaseHealthy && !messagingHealthy) ||
            (!storageHealthy && !messagingHealthy))
        {
            return (ApiErrorCodes.DependencyUnavailable, ApiErrorMessages.DependencyUnavailable);
        }

        if (!databaseHealthy)
        {
            return (ApiErrorCodes.DatabaseUnavailable, ApiErrorMessages.DatabaseUnavailable);
        }

        return !storageHealthy
            ? (ApiErrorCodes.StorageUnavailable, ApiErrorMessages.StorageUnavailable)
            : (ApiErrorCodes.DependencyUnavailable, ApiErrorMessages.DependencyUnavailable);
    }
}
