// File: backend/Services/Notification/NotificationService.Worker/Program.cs
// Mục đích: Khởi động Notification Worker, đăng ký Application/Infrastructure và cấu hình MassTransit consumers.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NotificationService.Application;
using NotificationService.Application.Contracts.Messaging;
using NotificationService.Infrastructure;
using NotificationService.Infrastructure.Persistence.Context;
using NotificationService.Worker.Consumers.NotificationBatches;
using BuildingBlocks.Observability.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Khởi tạo logging trước để mọi log từ bus/consumer có định danh Notification Service.
builder.AddLmsSerilog(
    ServiceNames.Notification);

// Worker cần database để đổi trạng thái batch, lưu recipient snapshot và ghi outbox message phát sinh sau snapshot.
var connectionString =
    builder.Configuration.GetConnectionString("Database")
    ?? throw new InvalidOperationException(
        "ConnectionStrings__Database is required for Notification Service Worker.");

builder.Services.AddNotificationApplication();

// Đăng ký repository, Student HTTP client và persistence adapter mà SnapshotNotificationBatchHandler dùng.
builder.Services.AddNotificationInfrastructure(
    builder.Configuration,
    connectionString);

builder.Services.AddLmsMessagingWithConsumers(
    builder.Configuration,
    ServiceNames.Notification,
    registration =>
    {
        // Consumer-side Entity Framework outbox giữ các command phát sinh trong lúc consume cho đến khi transaction database commit.
        registration.AddEntityFrameworkOutbox<NotificationDbContext>(outbox =>
            outbox.UseMySql());

        // Consumer này xử lý các token dispatch do snapshot tạo ra; mỗi token sẽ claim một chunk recipient riêng.
        registration.AddCommandConsumer<
            DispatchNotificationBatchConsumer,
            DispatchNotificationBatchV1>(
            ServiceNames.Notification);

        // Đăng ký contract SnapshotNotificationBatchV1 với SnapshotNotificationBatchConsumer.
        // Extension suy ra queue theo convention notification--snapshot-notification-batch-v1 và ConfigureEndpoints sẽ bind queue này với RabbitMQ.
        registration.AddCommandConsumer<
            SnapshotNotificationBatchConsumer,
            SnapshotNotificationBatchV1,
            SnapshotNotificationBatchConsumerDefinition>(
            ServiceNames.Notification);
    });

// RunAsync khởi động host và MassTransit bus; từ thời điểm này RabbitMQ giao message sẽ tự gọi Consume(...) của consumer tương ứng.
await builder.Build().RunAsync();
