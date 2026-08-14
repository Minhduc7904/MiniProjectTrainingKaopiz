using MassTransit;
using NotificationService.Application.Contracts.Messaging;
using NotificationService.Application.Features.Batches.Dispatch;

namespace NotificationService.Worker;

public sealed class DispatchNotificationBatchConsumer(DispatchNotificationBatchHandler handler) : IConsumer<DispatchNotificationBatchV1>
{
    public Task Consume(ConsumeContext<DispatchNotificationBatchV1> context) => handler.HandleAsync(context.Message, context.CancellationToken);
}
