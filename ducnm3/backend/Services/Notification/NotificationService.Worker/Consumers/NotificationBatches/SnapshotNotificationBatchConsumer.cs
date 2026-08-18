// File: backend/Services/Notification/NotificationService.Worker/Consumers/NotificationBatches/SnapshotNotificationBatchConsumer.cs
// Mục đích: Tiêu thụ message nền và kích hoạt nghiệp vụ SnapshotNotificationBatchConsumer.

using MassTransit;
using NotificationService.Application.Contracts.Messaging;
using NotificationService.Application.Features.Batches.Snapshot;

namespace NotificationService.Worker;

public sealed class SnapshotNotificationBatchConsumer(SnapshotNotificationBatchHandler handler)
    : IConsumer<SnapshotNotificationBatchV1>
{
    public Task Consume(ConsumeContext<SnapshotNotificationBatchV1> context) =>
        handler.HandleAsync(context.Message, context.CancellationToken);
}
