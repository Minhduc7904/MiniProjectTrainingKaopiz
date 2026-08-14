using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Api;
using BuildingBlocks.Presentation.Extensions;
using NSwag.AspNetCore;
using Yarp.ReverseProxy.Forwarder;

var builder = WebApplication.CreateBuilder(args);
var proxyMaxRequestBodySize = builder.Configuration.GetValue<long>(
    "Proxy:MaxRequestBodySize",
    525L * 1024 * 1024);
builder.WebHost.ConfigureKestrel(options =>
    options.Limits.MaxRequestBodySize = proxyMaxRequestBodySize);
builder.Services.AddHealthChecks();
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
RegisterServiceHttpClient(builder, ServiceNames.Course);
RegisterServiceHttpClient(builder, ServiceNames.Student);
RegisterServiceHttpClient(builder, ServiceNames.Media);
RegisterServiceHttpClient(builder, ServiceNames.Notification);
RegisterServiceHttpClient(builder, ServiceNames.Scheduler);
builder.Services.AddLmsCors(builder.Configuration);

var app = builder.Build();

app.UseLmsCors();
app.UseSharedApiMiddleware();

if (app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.UseSwaggerUi(settings =>
    {
        settings.Path = "/swagger";
        settings.SwaggerRoutes.Add(
            new SwaggerUiRoute("Course Service", $"{GatewayRoutePrefixes.Course}{ApiPaths.OpenApiDocument}"));
        settings.SwaggerRoutes.Add(
            new SwaggerUiRoute("Student Service", $"{GatewayRoutePrefixes.Student}{ApiPaths.OpenApiDocument}"));
        settings.SwaggerRoutes.Add(
            new SwaggerUiRoute("Media Service", $"{GatewayRoutePrefixes.Media}{ApiPaths.OpenApiDocument}"));
        settings.SwaggerRoutes.Add(
            new SwaggerUiRoute(
                "Notification Service",
                $"{GatewayRoutePrefixes.Notification}{ApiPaths.OpenApiDocument}"));
        settings.SwaggerRoutes.Add(
            new SwaggerUiRoute(
                "Scheduler Service",
                $"{GatewayRoutePrefixes.Scheduler}{ApiPaths.OpenApiDocument}"));
    });
}

app.MapGet("/", (HttpContext context) =>
        Results.Json(
            ApiResponseFactory.Success(
                new ServiceInfoResponse(ServiceNames.Gateway, HealthStatusValues.Healthy),
                context.TraceIdentifier)))
    .WithName("gateway-info")
    .WithTags(ServiceNames.Gateway)
    .Produces<ApiResponse<ServiceInfoResponse>>(StatusCodes.Status200OK);

app.MapGet(ApiPaths.Health, (HttpContext context) =>
        Results.Json(
            ApiResponseFactory.Success(
                new ServiceInfoResponse(ServiceNames.Gateway, HealthStatusValues.Healthy),
                context.TraceIdentifier)))
    .WithName("gateway-health")
    .WithTags(ServiceNames.Gateway)
    .Produces<ApiResponse<ServiceInfoResponse>>(StatusCodes.Status200OK);

app.MapGatewaySwaggerDocument(GatewayRoutePrefixes.Course, ServiceNames.Course);
app.MapGatewaySwaggerDocument(GatewayRoutePrefixes.Student, ServiceNames.Student);
app.MapGatewaySwaggerDocument(GatewayRoutePrefixes.Media, ServiceNames.Media);
app.MapGatewaySwaggerDocument(GatewayRoutePrefixes.Notification, ServiceNames.Notification);
app.MapGatewaySwaggerDocument(GatewayRoutePrefixes.Scheduler, ServiceNames.Scheduler);

app.MapReverseProxy(proxyPipeline =>
{
    proxyPipeline.Use(async (context, next) =>
    {
        await next();

        var forwarderError = context.Features.Get<IForwarderErrorFeature>();
        if (forwarderError is null ||
            forwarderError.Error == ForwarderError.None ||
            context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        await context.Response.WriteAsJsonAsync(
            ApiResponseFactory.Error(
                ApiErrorCodes.ServiceUnavailable,
                ApiErrorMessages.ServiceUnavailable,
                context.TraceIdentifier),
            context.RequestAborted);
    });
});

app.Run();

static void RegisterServiceHttpClient(WebApplicationBuilder builder, string serviceName)
{
    var address = builder.Configuration[$"{ConfigurationSectionNames.ServiceEndpoints}:{serviceName}"];
    if (string.IsNullOrWhiteSpace(address))
    {
        throw new InvalidOperationException(
            $"Missing service endpoint configuration for {serviceName}.");
    }

    builder.Services.AddHttpClient(serviceName, client =>
    {
        client.BaseAddress = new Uri(address);
        client.Timeout = TimeSpan.FromSeconds(5);
    });
}
