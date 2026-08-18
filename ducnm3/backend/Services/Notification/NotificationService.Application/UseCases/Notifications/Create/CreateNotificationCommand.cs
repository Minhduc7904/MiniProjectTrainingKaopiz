// File: backend/Services/Notification/NotificationService.Application/UseCases/Notifications/Create/CreateNotificationCommand.cs
// Mục đích: Mang dữ liệu đã map từ POST Notification vào Application mà không phụ thuộc HTTP request model.

namespace NotificationService.Application.UseCases.Notifications.Create;

public sealed record CreateNotificationCommand(
    Guid StudentId,
    string Title,
    string BodyMarkdown,
    Guid CreatedBy);
