using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Contracts.Health;
using BuildingBlocks.DatabaseMigration;
using BuildingBlocks.Messaging;
using BuildingBlocks.Presentation.Extensions;
using BuildingBlocks.Observability.Logging;
using Microsoft.Extensions.Logging;
using SchedulerService.Infrastructure.Health;

var builder = WebApplication.CreateBuilder(args);
builder.AddLmsSerilog(ServiceNames.Scheduler);
builder.Services.AddHealthChecks();
var migrationsRunOnly = builder.Configuration.GetValue<bool>("Migrations:RunOnly");
if (!migrationsRunOnly)
{
    builder.Services.AddLmsMessaging(builder.Configuration, ServiceNames.Scheduler);
}
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(document =>
{
    document.Title = "Scheduler Service API";
    document.Version = "v1";
});

var connectionString = builder.Configuration.GetConnectionString("Database");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings__Database environment variable is required for Scheduler Service.");
}

builder.Services.AddSingleton<IDatabaseHealthProbe>(serviceProvider =>
    new SchedulerDatabaseHealthProbe(
        connectionString,
        serviceProvider.GetRequiredService<ILogger<SchedulerDatabaseHealthProbe>>()));

var app = builder.Build();
var logMigration = LoggerMessage.Define<string>(
    LogLevel.Information,
    new EventId(1000, "SqlMigration"),
    "{MigrationMessage}");

await SqlMigrationRunner.ApplyAsync(
    new SqlMigrationRunnerOptions(
        ServiceNames.Scheduler,
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

app.MapServiceInfoEndpoint(ServiceNames.Scheduler);
app.MapDatabaseHealthEndpoint(ServiceNames.Scheduler);

app.Run();
