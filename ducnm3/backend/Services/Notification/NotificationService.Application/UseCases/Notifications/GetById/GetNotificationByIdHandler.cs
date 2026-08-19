// File: backend/Services/Notification/NotificationService.Application/UseCases/Notifications/GetById/GetNotificationByIdHandler.cs
// Mục đích: Đọc Notification theo ID và trả lỗi not-found chuẩn khi bản ghi không tồn tại.

using NotificationService.Application.Common.Errors;
using NotificationService.Application.Repositories;
using NotificationService.Application.Repositories.Models;

namespace NotificationService.Application.UseCases.Notifications.GetById;

public sealed class GetNotificationByIdHandler(INotificationRepository repository)
{
    public async Task<NotificationSummary> HandleAsync(
        Guid notificationId,
        CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(notificationId, cancellationToken) ??
        throw NotificationErrors.NotificationNotFound();
}
