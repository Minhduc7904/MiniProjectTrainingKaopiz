using System.Text.Json.Nodes;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;

namespace BuildingBlocks.Presentation.Extensions;

public static class GatewaySwaggerExtensions
{
    public static RouteHandlerBuilder MapGatewaySwaggerDocument(
        this IEndpointRouteBuilder endpoints,
        string gatewayPrefix,
        string serviceName)
    {
        return endpoints.MapGet(
                $"{gatewayPrefix}{ApiPaths.OpenApiDocument}",
                async (
                    HttpContext context,
                    IHttpClientFactory httpClientFactory,
                    CancellationToken cancellationToken) =>
                {
                    using var response = await httpClientFactory
                        .CreateClient(serviceName)
                        .GetAsync(ApiPaths.OpenApiDocument, cancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        return ServiceUnavailable(context);
                    }

                    var document = JsonNode.Parse(
                        await response.Content.ReadAsStringAsync(cancellationToken));
                    if (document is null)
                    {
                        return ServiceUnavailable(context);
                    }

                    document["servers"] = new JsonArray(
                        new JsonObject { ["url"] = gatewayPrefix });

                    return Results.Json(document);
                })
            .WithName($"{serviceName}-gateway-openapi");
    }

    private static IResult ServiceUnavailable(HttpContext context) =>
        Results.Json(
            ApiResponseFactory.Error(
                ApiErrorCodes.ServiceUnavailable,
                ApiErrorMessages.ServiceUnavailable,
                context.TraceIdentifier),
            statusCode: StatusCodes.Status503ServiceUnavailable);

}
