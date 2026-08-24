// File: backend/Services/Notification/NotificationService.Worker/Consumers/NotificationBatches/SnapshotNotificationBatchConsumer.cs
// Mục đích: Nhận SnapshotNotificationBatchV1 từ MassTransit và chuyển vào Snapshot handler để cố định recipient list.

using MassTransit;
using NotificationService.Application.Contracts.Messaging;
using NotificationService.Application.UseCases.NotificationBatches.Snapshot;

namespace NotificationService.Worker.Consumers.NotificationBatches;

public sealed class SnapshotNotificationBatchConsumer(SnapshotNotificationBatchHandler handler)
    : IConsumer<SnapshotNotificationBatchV1>
{
    // MassTransit gọi Consume tự động khi RabbitMQ giao SnapshotNotificationBatchV1 đến queue của Notification Service.
    // Không có polling thủ công: kiểu IConsumer<SnapshotNotificationBatchV1> là liên kết giữa message contract và code xử lý.
    // CancellationToken đến từ transport để Worker dừng/retry an toàn khi host shutdown hoặc message bị hủy.
    public Task Consume(ConsumeContext<SnapshotNotificationBatchV1> context) =>
        handler.HandleAsync(context.Message, context.CancellationToken);
}
