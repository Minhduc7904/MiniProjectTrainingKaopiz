// File: backend/Services/Notification/NotificationService.Application/UseCases/NotificationBatches/RetryFailed/RetryFailedNotificationBatchHandler.cs
// Mục đích: Tạo một batch con idempotent chỉ chứa recipient FAILED của batch nguồn đã kết thúc và phát lệnh snapshot.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using MediaService.Contracts.Messaging;
using NotificationService.Application.Common.Errors;
using NotificationService.Application.Contracts.Messaging;
using NotificationService.Application.Repositories;
using NotificationService.Application.Repositories.Models;
using NotificationService.Domain.Constants;

namespace NotificationService.Application.UseCases.NotificationBatches.RetryFailed;

public sealed class RetryFailedNotificationBatchHandler(
    INotificationBatchRepository repository,
    ICommandSender commandSender,
    TimeProvider timeProvider)
{
    public async Task<NotificationBatchSummary> HandleAsync(
        Guid sourceBatchId,
        Guid createdBy,
        CancellationToken cancellationToken)
    {
        if (sourceBatchId == Guid.Empty || createdBy == Guid.Empty)
        {
            throw NotificationErrors.Validation("batchId and createdBy must be valid UUIDs.");
        }

        var source = await repository.GetByIdAsync(sourceBatchId, cancellationToken)
            ?? throw NotificationErrors.BatchNotFound();
        if (source.Status is not (NotificationBatchStatuses.Completed or
            NotificationBatchStatuses.PartialFailed or NotificationBatchStatuses.Failed))
        {
            throw NotificationErrors.BatchNotTerminal();
        }
        if (source.FailedCount == 0)
        {
            throw NotificationErrors.BatchHasNoFailedItems();
        }

        var creation = await repository.PrepareRetryAsync(
            sourceBatchId, createdBy, timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
        if (!creation.IsNew)
        {
            return creation.Batch;
        }

        await commandSender.SendAsync(
            ServiceNames.Notification,
            new SnapshotNotificationBatchV1(creation.Batch.Id),
            cancellationToken);
        await commandSender.SendAsync(
            ServiceNames.Media,
            new StartNotificationMediaUsageJobV1(creation.Batch.Id),
            cancellationToken);
        return await repository.CommitRetryAsync(sourceBatchId, cancellationToken);
    }
}
