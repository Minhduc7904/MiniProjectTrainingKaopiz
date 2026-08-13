using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging;
using MassTransit;
using MediaService.Application;
using MediaService.Application.Contracts.Messaging;
using MediaService.Infrastructure;
using MediaService.Infrastructure.Persistence;
using MediaService.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
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
            .AddConsumer<
                GenerateMediaThumbnailFaultConsumer,
                GenerateMediaThumbnailFaultConsumerDefinition>()
            .Endpoint(endpoint =>
            {
                endpoint.Name =
                    "media-service--generate-media-thumbnail-v1-fault";
            });
    });

var host = builder.Build();
await host.RunAsync();
