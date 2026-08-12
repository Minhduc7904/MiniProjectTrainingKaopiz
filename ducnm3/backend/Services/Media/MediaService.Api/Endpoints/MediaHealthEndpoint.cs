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
                    CancellationToken cancellationToken) =>
                {
                    var databaseTask = databaseHealthProbe.CheckAsync(cancellationToken);
                    var storageTask = storageHealthProbe.CheckAsync(cancellationToken);
                    await Task.WhenAll(databaseTask, storageTask);

                    var databaseHealthy = databaseTask.Result.IsHealthy;
                    var storageHealthy = storageTask.Result.IsHealthy;

                    if (!databaseHealthy || !storageHealthy)
                    {
                        var (code, message) = SelectDependencyError(databaseHealthy, storageHealthy);
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
                                new StorageHealthResponse(HealthStatusValues.Healthy)),
                            context.TraceIdentifier));
                })
            .WithName($"{ServiceNames.Media}-health")
            .WithTags(ServiceNames.Media)
            .Produces<ApiResponse<MediaServiceHealthResponse>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status503ServiceUnavailable);
    }

    private static (string Code, string Message) SelectDependencyError(
        bool databaseHealthy,
        bool storageHealthy)
    {
        if (!databaseHealthy && !storageHealthy)
        {
            return (ApiErrorCodes.DependencyUnavailable, ApiErrorMessages.DependencyUnavailable);
        }

        return !databaseHealthy
            ? (ApiErrorCodes.DatabaseUnavailable, ApiErrorMessages.DatabaseUnavailable)
            : (ApiErrorCodes.StorageUnavailable, ApiErrorMessages.StorageUnavailable);
    }
}
