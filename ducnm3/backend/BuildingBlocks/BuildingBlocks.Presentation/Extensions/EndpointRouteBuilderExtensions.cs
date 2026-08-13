using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Contracts.Health;
using BuildingBlocks.Presentation.Api;

namespace BuildingBlocks.Presentation.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static RouteHandlerBuilder MapServiceInfoEndpoint(
        this IEndpointRouteBuilder endpoints,
        string serviceName)
    {
        return endpoints
            .MapGet("/", (HttpContext context) =>
                Results.Json(
                    ApiResponseFactory.Success(
                        new ServiceInfoResponse(serviceName, HealthStatusValues.Healthy),
                        context.TraceIdentifier)))
            .WithName($"{serviceName}-info")
            .WithTags(serviceName)
            .Produces<ApiResponse<ServiceInfoResponse>>(StatusCodes.Status200OK);
    }

    public static RouteHandlerBuilder MapDatabaseHealthEndpoint(
        this IEndpointRouteBuilder endpoints,
        string serviceName)
    {
        return endpoints
            .MapGet(
                ApiPaths.Health,
                async (
                    HttpContext context,
                    IDatabaseHealthProbe databaseHealthProbe,
                    IMessagingHealthProbe messagingHealthProbe,
                    CancellationToken cancellationToken) =>
                {
                    var databaseTask = databaseHealthProbe.CheckAsync(cancellationToken);
                    var messagingTask = messagingHealthProbe.CheckAsync(cancellationToken);
                    await Task.WhenAll(databaseTask, messagingTask);

                    if (!databaseTask.Result.IsHealthy || !messagingTask.Result.IsHealthy)
                    {
                        var databaseUnavailable = !databaseTask.Result.IsHealthy;
                        var messagingUnavailable = !messagingTask.Result.IsHealthy;
                        var onlyDatabaseUnavailable =
                            databaseUnavailable && !messagingUnavailable;
                        return Results.Json(
                            ApiResponseFactory.Error(
                                onlyDatabaseUnavailable
                                    ? ApiErrorCodes.DatabaseUnavailable
                                    : ApiErrorCodes.DependencyUnavailable,
                                onlyDatabaseUnavailable
                                    ? ApiErrorMessages.DatabaseUnavailable
                                    : ApiErrorMessages.DependencyUnavailable,
                                context.TraceIdentifier),
                            statusCode: StatusCodes.Status503ServiceUnavailable);
                    }

                    return Results.Json(
                        ApiResponseFactory.Success(
                            new ServiceHealthResponse(
                                serviceName,
                                HealthStatusValues.Healthy,
                                new DatabaseHealthResponse(HealthStatusValues.Healthy),
                                new MessagingHealthResponse(HealthStatusValues.Healthy)),
                            context.TraceIdentifier));
                })
            .WithName($"{serviceName}-health")
            .WithTags(serviceName)
            .Produces<ApiResponse<ServiceHealthResponse>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status503ServiceUnavailable);
    }
}
