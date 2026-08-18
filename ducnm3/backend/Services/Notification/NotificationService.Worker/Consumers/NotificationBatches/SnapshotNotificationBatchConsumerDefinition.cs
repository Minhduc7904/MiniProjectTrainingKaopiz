// File: backend/Services/Notification/NotificationService.Worker/Consumers/NotificationBatches/SnapshotNotificationBatchConsumerDefinition.cs
// Mục đích: Đặt queue, concurrency và retry policy MassTransit riêng cho consumer snapshot Notification Batch.

using MassTransit;

namespace NotificationService.Worker.Consumers.NotificationBatches;

public sealed class SnapshotNotificationBatchConsumerDefinition
    : ConsumerDefinition<SnapshotNotificationBatchConsumer>
{
    public SnapshotNotificationBatchConsumerDefinition()
    {
        ConcurrentMessageLimit = 1;
    }

}
