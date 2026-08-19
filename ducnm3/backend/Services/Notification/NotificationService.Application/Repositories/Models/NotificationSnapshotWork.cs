// File: backend/Services/Notification/NotificationService.Application/Repositories/Models/NotificationSnapshotWork.cs
// Mục đích: Cho Snapshot use case biết cần đọc recipient và có cần phát command dispatch hay không.

namespace NotificationService.Application.Repositories.Models;

public sealed record NotificationSnapshotWork(
    bool ShouldReadRecipients,
    bool ShouldDispatch,
    uint? RequestedCount = null,
    Guid? SourceBatchId = null,
    uint ExistingRecipientCount = 0);
