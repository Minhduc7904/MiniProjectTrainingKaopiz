// File: backend/Services/Notification/NotificationService.Application/UseCases/NotificationBatches/GetList/GetNotificationBatchesHandler.cs
// Mục đích: Validate bộ lọc và offset pagination, đọc danh sách batch rồi bổ sung thời lượng chạy cho trang quản lý.

using NotificationService.Application.Common.Errors;
using NotificationService.Application.Repositories;
using NotificationService.Application.Repositories.Models;
using NotificationService.Domain.Constants;

namespace NotificationService.Application.UseCases.NotificationBatches.GetList;

public sealed class GetNotificationBatchesHandler(
    INotificationBatchRepository repository,
    TimeProvider timeProvider)
{
    private static readonly HashSet<string> SupportedStatuses =
    [
        NotificationBatchStatuses.Pending,
        NotificationBatchStatuses.Snapshotting,
        NotificationBatchStatuses.SnapshotReady,
        NotificationBatchStatuses.Processing,
        NotificationBatchStatuses.Completed,
        NotificationBatchStatuses.PartialFailed,
        NotificationBatchStatuses.Failed,
    ];

    public async Task<NotificationBatchListPage> HandleAsync(
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var normalizedStatus = string.IsNullOrWhiteSpace(status)
            ? null
            : status.Trim().ToUpperInvariant();
        if (normalizedStatus is not null && !SupportedStatuses.Contains(normalizedStatus))
        {
            throw NotificationErrors.Validation("status is not supported.");
        }
        if (page < 1 || pageSize is < 1 or > 100 || page > int.MaxValue / pageSize)
        {
            throw NotificationErrors.Validation("page must be at least 1 and pageSize must be between 1 and 100.");
        }

        var result = await repository.ListAsync(
            normalizedStatus, page, pageSize, cancellationToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        return result with
        {
            Items = result.Items.Select(item => NotificationBatchDuration.Calculate(item, now)).ToArray(),
        };
    }
}
