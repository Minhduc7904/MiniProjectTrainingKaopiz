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
