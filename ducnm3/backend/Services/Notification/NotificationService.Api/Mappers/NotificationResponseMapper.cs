// File: backend/Services/Notification/NotificationService.Api/Mappers/NotificationResponseMapper.cs
// Mục đích: Chuyển Application summary thành HTTP response mà không để endpoint lặp mapping hoặc lộ persistence model.

using NotificationService.Api.Contracts.NotificationBatches.Responses;
using NotificationService.Api.Contracts.Notifications.Responses;
using NotificationService.Application.Repositories.Models;

namespace NotificationService.Api.Mappers;

public static class NotificationResponseMapper
{
    public static NotificationBatchResponse ToResponse(NotificationBatchSummary item) =>
        new(item.Id, item.Title, item.Status, item.TotalCount, item.ProcessedCount, item.SuccessCount,
            item.FailedCount, item.BatchSize, item.RequestedCount, item.SourceBatchId,
            item.CreatedAtUtc, item.StartedAtUtc, item.CompletedAtUtc, item.DurationMs);

    public static NotificationResponse ToResponse(NotificationSummary item) =>
        new(item.Id, item.RecipientStudentId, item.Title, item.BodyMarkdown, item.SourceType,
            item.Status, item.CreatedBy, item.CreatedAtUtc, item.ReadAtUtc);
}
