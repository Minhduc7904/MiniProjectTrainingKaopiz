// File: backend/Services/Notification/NotificationService.Worker/Consumers/NotificationBatches/DispatchNotificationBatchConsumer.cs
// Mục đích: Tiêu thụ message nền và kích hoạt nghiệp vụ DispatchNotificationBatchConsumer.

using MassTransit;
using NotificationService.Application.Contracts.Messaging;
using NotificationService.Application.Features.Batches.Dispatch;

namespace NotificationService.Worker;

public sealed class DispatchNotificationBatchConsumer(DispatchNotificationBatchHandler handler) : IConsumer<DispatchNotificationBatchV1>
{
    public Task Consume(ConsumeContext<DispatchNotificationBatchV1> context) => handler.HandleAsync(context.Message, context.CancellationToken);
}
