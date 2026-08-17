using System.Text.Json;
using System.Text.Json.Nodes;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;

namespace BuildingBlocks.Presentation.Extensions;

/// <summary>Proxy OpenAPI document của downstream service qua Gateway và thay server URL thành prefix public.</summary>
public static class GatewaySwaggerExtensions
{
    /// <summary>
    /// Map endpoint swagger của service tại <paramref name="gatewayPrefix"/>. Client HTTP được lấy theo <paramref name="serviceName"/>.
    /// Khi downstream, network hoặc JSON lỗi, method trả error envelope 503 thay vì chuyển tiếp lỗi kỹ thuật.
    /// </summary>
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
                    try
                    {
                        // Đọc document từ downstream, sau đó sửa trường servers để Try it out gọi lại Gateway thay vì bypass nó.
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
                    }
                    // Các lỗi dependency đều được quy về response service-unavailable an toàn cho client.
                    catch (HttpRequestException)
                    {
                        return ServiceUnavailable(context);
                    }
                    catch (JsonException)
                    {
                        return ServiceUnavailable(context);
                    }
                    catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                    {
                        return ServiceUnavailable(context);
                    }
                })
            .WithName($"{serviceName}-gateway-openapi");
    }

    /// <summary>Tạo response 503 chuẩn dùng cho mọi nhánh proxy Swagger thất bại.</summary>
    private static IResult ServiceUnavailable(HttpContext context) =>
        Results.Json(
            ApiResponseFactory.Error(
                ApiErrorCodes.ServiceUnavailable,
                ApiErrorMessages.ServiceUnavailable,
                context.TraceIdentifier),
            statusCode: StatusCodes.Status503ServiceUnavailable);

}
