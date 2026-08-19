// File: backend/Services/Notification/NotificationService.Application/Repositories/Models/CreateNotificationBatchRecord.cs
// Mục đích: Mang dữ liệu tạo Notification Batch từ Application đến repository với ID và thời điểm đã xác định.

namespace NotificationService.Application.Repositories.Models;

public sealed record CreateNotificationBatchRecord(
    Guid Id,
    string Title,
    string BodyMarkdown,
    string TargetScope,
    Guid CreatedBy,
    uint BatchSize,
    uint? RequestedCount,
    Guid? SourceBatchId,
    DateTime CreatedAtUtc);
