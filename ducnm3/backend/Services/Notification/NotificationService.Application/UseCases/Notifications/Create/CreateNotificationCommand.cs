// File: backend/Services/Notification/NotificationService.Application/UseCases/Notifications/Create/CreateNotificationCommand.cs
// Mục đích: Định nghĩa dữ liệu đầu vào cho use case CreateNotificationCommand.

namespace NotificationService.Application.Features.Notifications.Create;

public sealed record CreateNotificationCommand(
    Guid StudentId,
    string Title,
    string BodyMarkdown,
    Guid CreatedBy);
