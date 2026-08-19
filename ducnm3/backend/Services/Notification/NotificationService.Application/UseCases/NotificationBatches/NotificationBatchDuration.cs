// File: backend/Services/Notification/NotificationService.Application/UseCases/NotificationBatches/NotificationBatchDuration.cs
// Mục đích: Tính thời lượng batch nhất quán cho detail và list từ thời điểm bắt đầu đến hiện tại hoặc khi hoàn tất.

using NotificationService.Application.Repositories.Models;

namespace NotificationService.Application.UseCases.NotificationBatches;

public static class NotificationBatchDuration
{
    public static NotificationBatchSummary Calculate(NotificationBatchSummary batch, DateTime nowUtc)
    {
        if (batch.StartedAtUtc is null)
        {
            return batch with { DurationMs = null };
        }

        var end = batch.CompletedAtUtc ?? nowUtc;
        return batch with
        {
            DurationMs = Math.Max(0, checked((long)(end - batch.StartedAtUtc.Value).TotalMilliseconds)),
        };
    }
}
