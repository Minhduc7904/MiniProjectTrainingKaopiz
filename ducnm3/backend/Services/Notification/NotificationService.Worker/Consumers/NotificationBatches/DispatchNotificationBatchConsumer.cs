// File: backend/Services/Notification/NotificationService.Worker/Consumers/NotificationBatches/DispatchNotificationBatchConsumer.cs
// Mục đích: Nhận DispatchNotificationBatchV1 từ MassTransit và chuyển vào Dispatch handler để xử lý một chunk recipient.

using MassTransit;
using NotificationService.Application.Contracts.Messaging;
using NotificationService.Application.UseCases.NotificationBatches.Dispatch;

namespace NotificationService.Worker.Consumers.NotificationBatches;

public sealed class DispatchNotificationBatchConsumer(DispatchNotificationBatchHandler handler) : IConsumer<DispatchNotificationBatchV1>
{
    // Đây là chặng sau snapshot: message được handler snapshot gửi vào queue dispatch.
    // MassTransit chọn consumer này theo generic IConsumer<DispatchNotificationBatchV1>,
    // rồi truyền toàn bộ envelope (message, correlation id, retry context, cancellation) qua ConsumeContext.
    // Handler bên dưới claim item bằng lease, do đó nhiều dispatch message vẫn không xử lý cùng một item đồng thời.
    public Task Consume(ConsumeContext<DispatchNotificationBatchV1> context) => handler.HandleAsync(context.Message, context.CancellationToken);
}
