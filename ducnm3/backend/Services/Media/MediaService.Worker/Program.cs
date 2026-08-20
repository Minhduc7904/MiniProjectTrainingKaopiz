// File: backend/Services/Media/MediaService.Worker/Program.cs
// Mục đích: Composition root khởi tạo host, đăng ký dependency và map transport của service.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging;
using MassTransit;
using MediaService.Application;
using MediaService.Application.Contracts.Messaging;
using MediaService.Contracts.Messaging;
using MediaService.Infrastructure;
using MediaService.Infrastructure.Persistence.Context;
using MediaService.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Observability.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.AddLmsSerilog(
    ServiceNames.Media);

var connectionString = builder.Configuration.GetConnectionString("Database");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings__Database is required for Media Service Worker.");
}

builder.Services.AddMediaApplication(builder.Configuration);

builder.Services.AddMediaInfrastructure(
    builder.Configuration,
    connectionString);

builder.Services.AddLmsMessagingWithConsumers(
    builder.Configuration,
    ServiceNames.Media,
    registration =>
    {
        registration.AddEntityFrameworkOutbox<MediaDbContext>(outbox =>
            outbox.UseMySql());

        registration
            .AddCommandConsumer<
                GenerateMediaThumbnailConsumer,
                GenerateMediaThumbnailV1,
                GenerateMediaThumbnailConsumerDefinition>(
                ServiceNames.Media);

        registration
            .AddCommandConsumer<
                RegisterNotificationMediaUsageConsumer,
                RegisterNotificationMediaUsageV1,
                RegisterNotificationMediaUsageConsumerDefinition>(
                ServiceNames.Media);

        registration
            .AddCommandConsumer<
                RegisterCourseLessonMediaUsageConsumer,
                RegisterCourseLessonMediaUsageV1,
                RegisterCourseLessonMediaUsageConsumerDefinition>(
                ServiceNames.Media);

        registration
            .AddCommandConsumer<
                RegisterNotificationMediaUsageBatchConsumer,
                RegisterNotificationMediaUsageBatchV1,
                RegisterNotificationMediaUsageBatchConsumerDefinition>(
                ServiceNames.Media);

        registration
            .AddCommandConsumer<
                StartNotificationMediaUsageJobConsumer,
                StartNotificationMediaUsageJobV1,
                NotificationMediaUsageJobCommandConsumerDefinition<
                    StartNotificationMediaUsageJobConsumer>>(
                ServiceNames.Media);

        registration
            .AddCommandConsumer<
                CompleteNotificationMediaUsageJobConsumer,
                CompleteNotificationMediaUsageJobV1,
                NotificationMediaUsageJobCommandConsumerDefinition<
                    CompleteNotificationMediaUsageJobConsumer>>(
                ServiceNames.Media);

        registration
            .AddConsumer<
                GenerateMediaThumbnailFaultConsumer,
                GenerateMediaThumbnailFaultConsumerDefinition>()
            .Endpoint(endpoint =>
            {
                endpoint.Name =
                    "media-service--generate-media-thumbnail-v1-fault";
            });

        registration
            .AddConsumer<
                RegisterNotificationMediaUsageFaultConsumer,
                RegisterNotificationMediaUsageFaultConsumerDefinition>()
            .Endpoint(endpoint =>
            {
                endpoint.Name =
                    "media-service--register-notification-media-usage-v1-fault";
            });
    });

var host = builder.Build();

await host.RunAsync();