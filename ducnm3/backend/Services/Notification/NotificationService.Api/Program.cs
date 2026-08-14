using BuildingBlocks.DatabaseMigration;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Contracts.Health;
using BuildingBlocks.Messaging;
using BuildingBlocks.Presentation.Extensions;
using NotificationService.Application;
using NotificationService.Infrastructure;
using NotificationService.Api.Endpoints;
using NotificationService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();
var migrationsRunOnly = builder.Configuration.GetValue<bool>("Migrations:RunOnly");
if (!migrationsRunOnly)
{
    builder.Services.AddLmsMessaging(
        builder.Configuration,
        ServiceNames.Notification,
        registration => registration
            .AddEntityFrameworkOutbox<NotificationDbContext>(outbox =>
            {
                outbox.UseMySql();
                outbox.UseBusOutbox();
            }));
}
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(document =>
{
    document.Title = "Notification Service API";
    document.Version = "v1";
});

var connectionString = builder.Configuration.GetConnectionString("Database");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings__Database environment variable is required for Notification Service.");
}

if (!migrationsRunOnly)
{
    builder.Services.AddNotificationApplication();
    builder.Services.AddNotificationInfrastructure(builder.Configuration, connectionString);
}

var app = builder.Build();
var logMigration = LoggerMessage.Define<string>(
    LogLevel.Information,
    new EventId(1000, "SqlMigration"),
    "{MigrationMessage}");

await SqlMigrationRunner.ApplyAsync(
    new SqlMigrationRunnerOptions(
        ServiceNames.Notification,
        connectionString,
        Path.Combine(app.Environment.ContentRootPath, "Database", "Migrations")),
    message => logMigration(app.Logger, message, null));

if (migrationsRunOnly)
{
    return;
}

app.UseSharedApiMiddleware();

if (app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.UseOpenApi();
    app.UseSwaggerUi(settings => settings.Path = "/swagger");
}

app.MapServiceInfoEndpoint(ServiceNames.Notification);
app.MapDatabaseHealthEndpoint(ServiceNames.Notification);
app.MapNotificationBatchEndpoints();

app.Run();
