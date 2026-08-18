// File: backend/Services/Notification/NotificationService.Application/Repositories/Models/NotificationBatchWorkItem.cs
// Mục đích: Mang dữ liệu item đã claim từ persistence đến Dispatch use case để gửi notification.

namespace NotificationService.Application.Repositories.Models;

public sealed record NotificationBatchWorkItem(Guid Id, Guid BatchId, Guid StudentId,
    uint RetryCount, string Title, string BodyMarkdown, Guid CreatedBy);
