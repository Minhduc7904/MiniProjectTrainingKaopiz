using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Contracts.Health;
using BuildingBlocks.Presentation.Api;

namespace BuildingBlocks.Presentation.Extensions;

/// <summary>Map các endpoint hạ tầng nhất quán cho service: thông tin service và readiness health check.</summary>
public static class EndpointRouteBuilderExtensions
{
    /// <summary>Map <c>GET /</c>, trả service name và trạng thái healthy trong response envelope.</summary>
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

    /// <summary>
    /// Map <c>GET /health</c>; gọi database và messaging probe song song rồi trả 200 hoặc 503 với error code chuẩn.
    /// <paramref name="serviceName"/> được đưa vào response, name và OpenAPI tag của endpoint.
    /// </summary>
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
                    // Hai dependency độc lập nên được kiểm tra đồng thời để giảm thời gian readiness check.
                    var databaseTask = databaseHealthProbe.CheckAsync(cancellationToken);
                    var messagingTask = messagingHealthProbe.CheckAsync(cancellationToken);
                    await Task.WhenAll(databaseTask, messagingTask);

                    // Chỉ database lỗi dùng DATABASE_UNAVAILABLE; messaging hoặc nhiều dependency lỗi dùng DEPENDENCY_UNAVAILABLE.
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
