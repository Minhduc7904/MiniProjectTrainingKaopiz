// File: backend/Services/Notification/NotificationService.Worker/Consumers/NotificationBatches/SnapshotNotificationBatchConsumerDefinition.cs
// Mục đích: Đặt queue, concurrency và retry policy MassTransit riêng cho consumer snapshot Notification Batch.

using MassTransit;

namespace NotificationService.Worker.Consumers.NotificationBatches;

public sealed class SnapshotNotificationBatchConsumerDefinition
    : ConsumerDefinition<SnapshotNotificationBatchConsumer>
{
    public SnapshotNotificationBatchConsumerDefinition()
    {
        // Một snapshot đọc nhiều page Student và ghi nhiều batch item; xử lý tuần tự ở queue này giảm việc hai message
        // cùng snapshot một batch. Repository vẫn phải idempotent vì RabbitMQ có thể redeliver message.
        ConcurrentMessageLimit = 1;
    }

}
