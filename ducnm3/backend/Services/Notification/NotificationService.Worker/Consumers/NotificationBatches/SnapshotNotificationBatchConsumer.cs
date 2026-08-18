// File: backend/Services/Notification/NotificationService.Worker/Consumers/NotificationBatches/SnapshotNotificationBatchConsumer.cs
// Mục đích: Nhận SnapshotNotificationBatchV1 từ MassTransit và chuyển vào Snapshot handler để cố định recipient list.

using MassTransit;
using NotificationService.Application.Contracts.Messaging;
using NotificationService.Application.UseCases.NotificationBatches.Snapshot;

namespace NotificationService.Worker.Consumers.NotificationBatches;

public sealed class SnapshotNotificationBatchConsumer(SnapshotNotificationBatchHandler handler)
    : IConsumer<SnapshotNotificationBatchV1>
{
    public Task Consume(ConsumeContext<SnapshotNotificationBatchV1> context) =>
        handler.HandleAsync(context.Message, context.CancellationToken);
}
