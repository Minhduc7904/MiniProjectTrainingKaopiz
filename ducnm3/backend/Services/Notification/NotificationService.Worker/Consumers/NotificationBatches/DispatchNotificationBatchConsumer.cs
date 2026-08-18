// File: backend/Services/Notification/NotificationService.Worker/Consumers/NotificationBatches/DispatchNotificationBatchConsumer.cs
// Mục đích: Nhận DispatchNotificationBatchV1 từ MassTransit và chuyển vào Dispatch handler để xử lý một chunk recipient.

using MassTransit;
using NotificationService.Application.Contracts.Messaging;
using NotificationService.Application.UseCases.NotificationBatches.Dispatch;

namespace NotificationService.Worker.Consumers.NotificationBatches;

public sealed class DispatchNotificationBatchConsumer(DispatchNotificationBatchHandler handler) : IConsumer<DispatchNotificationBatchV1>
{
    public Task Consume(ConsumeContext<DispatchNotificationBatchV1> context) => handler.HandleAsync(context.Message, context.CancellationToken);
}
