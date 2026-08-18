// File: backend/Services/Notification/NotificationService.Application/UseCases/NotificationBatches/Create/CreateNotificationBatchCommand.cs
// Mục đích: Định nghĩa dữ liệu đầu vào cho use case CreateNotificationBatchCommand.

namespace NotificationService.Application.Features.Batches.Create;

public sealed record CreateNotificationBatchCommand(string Title, string BodyMarkdown, string TargetScope, Guid CreatedBy, uint? BatchSize, Guid? CourseId);
