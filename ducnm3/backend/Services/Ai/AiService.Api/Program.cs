// File: backend/Services/Ai/AiService.Api/Program.cs
// Mục đích: Composition root khởi tạo host, đăng ký dependency và map transport của service.

using AiService.Api.Endpoints.Bindings.DeleteBinding;
using AiService.Api.Endpoints.Bindings.GetBinding;
using AiService.Api.Endpoints.Bindings.GetBindings;
using AiService.Api.Endpoints.Bindings.UpsertBinding;
using AiService.Api.Endpoints.Completions.CreateCompletion;
using AiService.Api.Endpoints.Models.CreateModel;
using AiService.Api.Endpoints.Models.DeleteModel;
using AiService.Api.Endpoints.Models.GetModelDetail;
using AiService.Api.Endpoints.Models.GetModels;
using AiService.Api.Endpoints.Models.UpdateModel;
using AiService.Api.Endpoints.Options.GetOptions;
using AiService.Api.Endpoints.Providers.CreateProvider;
using AiService.Api.Endpoints.Providers.DeleteProvider;
using AiService.Api.Endpoints.Providers.GetProviderDetail;
using AiService.Api.Endpoints.Providers.GetProviders;
using AiService.Api.Endpoints.Providers.HealthCheck;
using AiService.Api.Endpoints.Providers.UpdateProvider;
using AiService.Application;
using AiService.Infrastructure;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.DatabaseMigration;
using BuildingBlocks.Messaging;
using BuildingBlocks.Observability.Logging;
using BuildingBlocks.Presentation.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.AddLmsSerilog(ServiceNames.Ai);
builder.Services.AddHealthChecks();
var migrationsRunOnly = builder.Configuration.GetValue<bool>("Migrations:RunOnly");
if (!migrationsRunOnly)
{
    builder.Services.AddLmsMessaging(builder.Configuration, ServiceNames.Ai);
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(document =>
{
    document.Title = "AI Service API";
    document.Version = "v1";
});

var connectionString = builder.Configuration.GetConnectionString("Database");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings__Database environment variable is required for AI Service.");
}

if (!migrationsRunOnly)
{
    builder.Services.AddAiApplication();
    builder.Services.AddAiInfrastructure(builder.Configuration, connectionString);
}

var app = builder.Build();
var logMigration = LoggerMessage.Define<string>(
    LogLevel.Information,
    new EventId(1000, "SqlMigration"),
    "{MigrationMessage}");

await SqlMigrationRunner.ApplyAsync(
    new SqlMigrationRunnerOptions(
        ServiceNames.Ai,
        connectionString,
        Path.Combine(app.Environment.ContentRootPath, "Database", "Migrations")),
    message => logMigration(app.Logger, message, null));

if (migrationsRunOnly)
{
    return;
}

app.UseSharedApiMiddleware();
app.UseLmsHttpLogging();

if (app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.UseOpenApi();
    app.UseSwaggerUi(settings => settings.Path = "/swagger");
}

app.MapServiceInfoEndpoint(ServiceNames.Ai);
app.MapDatabaseHealthEndpoint(ServiceNames.Ai);
app.MapGetAiOptions();
app.MapCreateAiCompletion();
app.MapCreateAiProvider();
app.MapGetAiProviders();
app.MapGetAiProviderDetail();
app.MapUpdateAiProvider();
app.MapDeleteAiProvider();
app.MapCheckAiProvidersHealth();
app.MapCheckAiProviderHealth();
app.MapCreateAiModel();
app.MapGetAiModels();
app.MapGetAiModelDetail();
app.MapUpdateAiModel();
app.MapDeleteAiModel();
app.MapGetAiServiceBindings();
app.MapGetAiServiceBinding();
app.MapUpsertAiServiceBinding();
app.MapDeleteAiServiceBinding();

app.Run();
