// File: backend/Services/Notification/NotificationService.Application/Repositories/INotificationBatchDispatchRepository.cs
// Mục đích: Định nghĩa riêng persistence port cho claim lease, hoàn tất delivery và finalize Notification Batch dispatch.

using NotificationService.Application.Repositories.Models;

namespace NotificationService.Application.Repositories;

public interface INotificationBatchDispatchRepository
{
    Task<NotificationBatchClaim?> ClaimChunkAsync(Guid batchId, CancellationToken cancellationToken);

    Task<IReadOnlyList<NotificationSummary>> CompleteClaimAsync(
        NotificationBatchClaim claim,
        IReadOnlyList<NotificationBatchDeliveryResult> results,
        CancellationToken cancellationToken);

    Task<NotificationBatchContinuation> FinalizeOrHasRemainingAsync(Guid batchId, CancellationToken cancellationToken);
}
