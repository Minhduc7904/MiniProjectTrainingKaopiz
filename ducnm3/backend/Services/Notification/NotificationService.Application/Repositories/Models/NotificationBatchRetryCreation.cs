// File: backend/Services/Notification/NotificationService.Application/Repositories/Models/NotificationBatchRetryCreation.cs
// Mục đích: Cho Retry use case biết batch con là candidate mới cần ghi outbox hay là child idempotent đã tồn tại.

namespace NotificationService.Application.Repositories.Models;

public sealed record NotificationBatchRetryCreation(
    NotificationBatchSummary Batch,
    bool IsNew);
