using BuildingBlocks.DatabaseMigration;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Presentation.Extensions;
using MediaService.Api.Endpoints;
using MediaService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(document =>
{
    document.Title = "Media Service API";
    document.Version = "v1";
});

var connectionString = builder.Configuration.GetConnectionString("Database");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings__Database environment variable is required for Media Service.");
}

builder.Services.AddMediaInfrastructure(builder.Configuration, connectionString);

var app = builder.Build();
var logMigration = LoggerMessage.Define<string>(
    LogLevel.Information,
    new EventId(1000, "SqlMigration"),
    "{MigrationMessage}");

await SqlMigrationRunner.ApplyAsync(
    new SqlMigrationRunnerOptions(
        ServiceNames.Media,
        connectionString,
        Path.Combine(app.Environment.ContentRootPath, "Database", "Migrations")),
    message => logMigration(app.Logger, message, null));

if (builder.Configuration.GetValue<bool>("Migrations:RunOnly"))
{
    return;
}

app.UseSharedApiMiddleware();

if (app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.UseOpenApi();
    app.UseSwaggerUi(settings => settings.Path = "/swagger");
}

app.MapServiceInfoEndpoint(ServiceNames.Media);
app.MapMediaHealthEndpoint();

app.Run();
