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
                    CancellationToken cancellationToken) =>
                {
                    var database = await databaseHealthProbe.CheckAsync(cancellationToken);
                    if (!database.IsHealthy)
                    {
                        return Results.Json(
                            ApiResponseFactory.Error(
                                ApiErrorCodes.DatabaseUnavailable,
                                ApiErrorMessages.DatabaseUnavailable,
                                context.TraceIdentifier),
                            statusCode: StatusCodes.Status503ServiceUnavailable);
                    }

                    return Results.Json(
                        ApiResponseFactory.Success(
                            new ServiceHealthResponse(
                                serviceName,
                                HealthStatusValues.Healthy,
                                new DatabaseHealthResponse(HealthStatusValues.Healthy)),
                            context.TraceIdentifier));
                })
            .WithName($"{serviceName}-health")
            .WithTags(serviceName)
            .Produces<ApiResponse<ServiceHealthResponse>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status503ServiceUnavailable);
    }
}
