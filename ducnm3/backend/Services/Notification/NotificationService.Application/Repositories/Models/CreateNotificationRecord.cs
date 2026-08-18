// File: backend/Services/Notification/NotificationService.Application/Repositories/Models/CreateNotificationRecord.cs
// Mục đích: Mang dữ liệu đã validate từ use case Create Notification đến repository ghi persistence.

namespace NotificationService.Application.Repositories.Models;

public sealed record CreateNotificationRecord(Guid Id, Guid RecipientStudentId, string Title,
    string BodyMarkdown, Guid CreatedBy, DateTime CreatedAtUtc);
