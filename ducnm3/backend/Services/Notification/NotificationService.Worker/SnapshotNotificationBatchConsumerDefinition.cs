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
