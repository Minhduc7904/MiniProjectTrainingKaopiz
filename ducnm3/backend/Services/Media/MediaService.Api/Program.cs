using BuildingBlocks.DatabaseMigration;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(document =>
{
    document.Title = "Media Service API";
    document.Version = "v1";
});

var app = builder.Build();
var logMigration = LoggerMessage.Define<string>(
    LogLevel.Information,
    new EventId(1000, "SqlMigration"),
    "{MigrationMessage}");

var connectionString = builder.Configuration.GetConnectionString("Database");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings__Database environment variable is required for Media Service.");
}

await SqlMigrationRunner.ApplyAsync(
    new SqlMigrationRunnerOptions(
        "media-service",
        connectionString,
        Path.Combine(app.Environment.ContentRootPath, "Database", "Migrations")),
    message => logMigration(app.Logger, message, null));

if (builder.Configuration.GetValue<bool>("Migrations:RunOnly"))
{
    return;
}

if (app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.UseOpenApi();
    app.UseSwaggerUi(settings => settings.Path = "/swagger");
}

app.MapGet("/", () => Results.Ok(new { service = "media-service", status = "ready" }));
app.MapHealthChecks("/health");

app.Run();
