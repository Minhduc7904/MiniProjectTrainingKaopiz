// File: backend/Services/Notification/NotificationService.Worker/Consumers/NotificationBatches/SnapshotNotificationBatchConsumerDefinition.cs
// Mục đích: Cấu hình endpoint và retry policy MassTransit cho consumer SnapshotNotificationBatchConsumerDefinition.

using MassTransit;

namespace NotificationService.Worker;

public sealed class SnapshotNotificationBatchConsumerDefinition
    : ConsumerDefinition<SnapshotNotificationBatchConsumer>
{
    public SnapshotNotificationBatchConsumerDefinition()
    {
        ConcurrentMessageLimit = 1;
    }

}
