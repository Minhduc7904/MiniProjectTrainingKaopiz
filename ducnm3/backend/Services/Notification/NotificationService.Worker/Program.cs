using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging;
using NotificationService.Application;
using NotificationService.Application.Contracts.Messaging;
using NotificationService.Infrastructure;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Worker;
using MassTransit;

var builder = Host.CreateApplicationBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Database") ?? throw new InvalidOperationException("ConnectionStrings__Database is required for Notification Service Worker.");
builder.Services.AddNotificationApplication();
builder.Services.AddNotificationInfrastructure(builder.Configuration, connectionString);
builder.Services.AddLmsMessagingWithConsumers(
    builder.Configuration,
    ServiceNames.Notification,
    registration =>
    {
        registration.AddEntityFrameworkOutbox<NotificationDbContext>(outbox =>
            outbox.UseMySql());
        registration.AddCommandConsumer<
            DispatchNotificationBatchConsumer,
            DispatchNotificationBatchV1,
            DispatchNotificationBatchConsumerDefinition>(ServiceNames.Notification);
    });
await builder.Build().RunAsync();
