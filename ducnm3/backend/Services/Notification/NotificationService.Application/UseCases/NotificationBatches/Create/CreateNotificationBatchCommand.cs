// File: backend/Services/Notification/NotificationService.Application/UseCases/NotificationBatches/Create/CreateNotificationBatchCommand.cs
// Mục đích: Mang dữ liệu đã map từ POST Notification Batch vào Application mà không phụ thuộc HTTP contract.

namespace NotificationService.Application.UseCases.NotificationBatches.Create;

public sealed record CreateNotificationBatchCommand(
    string Title,
    string BodyMarkdown,
    string TargetScope,
    Guid CreatedBy,
    uint? BatchSize,
    uint? RequestedCount,
    Guid? CourseId);
