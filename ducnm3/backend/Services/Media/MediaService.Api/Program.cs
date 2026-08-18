// File: backend/Services/Media/MediaService.Api/Program.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using BuildingBlocks.DatabaseMigration;
using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging;
using BuildingBlocks.Presentation.Extensions;
using MediaService.Api.Endpoints;
using MediaService.Api.Endpoints.Media;
using MediaService.Application;
using MediaService.Application.UseCases.Media.Upload;
using MediaService.Infrastructure;
using MediaService.Infrastructure.Persistence.Context;
using MassTransit;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);
var mediaRequestMaxBytes = builder.Configuration.GetValue<long>(
    $"{MediaUploadOptions.SectionName}:RequestMaxBytes",
    525L * 1024 * 1024);
builder.WebHost.ConfigureKestrel(options =>
    options.Limits.MaxRequestBodySize = mediaRequestMaxBytes);
builder.Services.Configure<FormOptions>(options =>
    options.MultipartBodyLengthLimit = mediaRequestMaxBytes);
builder.Services.AddHealthChecks();
var migrationsRunOnly = builder.Configuration.GetValue<bool>("Migrations:RunOnly");
if (!migrationsRunOnly)
{
    builder.Services.AddLmsMessaging(
        builder.Configuration,
        ServiceNames.Media,
        registration => registration
            .AddEntityFrameworkOutbox<MediaDbContext>(outbox =>
            {
                outbox.UseMySql();
                outbox.UseBusOutbox();
            }));
}
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

if (!migrationsRunOnly)
{
    builder.Services.AddMediaApplication(builder.Configuration);
    builder.Services.AddMediaInfrastructure(builder.Configuration, connectionString);
}

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

app.MapServiceInfoEndpoint(ServiceNames.Media);
app.MapMediaHealthEndpoint();
app.MapUploadMedia();
app.MapCreateUploadIntent();
app.MapCompleteDirectUpload();
app.MapCreateMediaUsage();
app.MapGetMediaUsageUrl();
app.MapGetMediaUsageUrls();
app.MapGetMediaContent();
app.MapGetMediaThumbnailStatus();
app.MapRetryMediaThumbnail();

app.Run();
