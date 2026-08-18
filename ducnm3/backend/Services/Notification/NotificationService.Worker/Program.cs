// File: backend/Services/Notification/NotificationService.Worker/Program.cs
// Mục đích: Khởi động Notification Worker, đăng ký Application/Infrastructure và cấu hình MassTransit consumers.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging;
using NotificationService.Application;
using NotificationService.Application.Contracts.Messaging;
using NotificationService.Infrastructure;
using NotificationService.Infrastructure.Persistence.Context;
using NotificationService.Worker.Consumers.NotificationBatches;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

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
        registration.AddCommandConsumer<DispatchNotificationBatchConsumer, DispatchNotificationBatchV1>(ServiceNames.Notification);
        registration.AddCommandConsumer<
            SnapshotNotificationBatchConsumer,
            SnapshotNotificationBatchV1,
            SnapshotNotificationBatchConsumerDefinition>(ServiceNames.Notification);
    });
await builder.Build().RunAsync();
