// File: backend/Services/Notification/NotificationService.Application/UseCases/NotificationBatches/Snapshot/SnapshotNotificationBatchHandler.cs
// Mục đích: Đọc student theo page, lưu snapshot recipient idempotent và phát các command dispatch theo concurrency cấu hình.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using NotificationService.Application.Repositories;
using NotificationService.Application.Repositories.Models;
using NotificationService.Application.Services.Students;
using NotificationService.Application.Common.Errors;
using NotificationService.Application.UseCases.NotificationBatches;
using NotificationService.Application.Contracts.Messaging;

namespace NotificationService.Application.UseCases.NotificationBatches.Snapshot;

public sealed class SnapshotNotificationBatchHandler(
    IStudentRecipientClient studentRecipientClient,
    INotificationBatchRepository repository,
    ICommandSender commandSender,
    NotificationBatchProcessingOptions options)
{
    public async Task HandleAsync(
        SnapshotNotificationBatchV1 command,
        CancellationToken cancellationToken)
    {
        var work = await repository.PrepareSnapshotAsync(command.BatchId, cancellationToken);
        if (work.ShouldReadRecipients)
        {
            try
            {
                await foreach (var page in studentRecipientClient
                    .GetActiveStudentIdPagesAsync(cancellationToken)
                    .WithCancellation(cancellationToken))
                {
                    await repository.AppendSnapshotPageAsync(
                        command.BatchId,
                        page.Distinct().ToArray(),
                        cancellationToken);
                }

                if (!await repository.CompleteSnapshotAsync(command.BatchId, cancellationToken))
                {
                    return;
                }
            }
            catch (NotificationApplicationException)
            {
                await repository.MarkSnapshotFailedAsync(command.BatchId, cancellationToken);
                return;
            }
        }

        if (work.ShouldDispatch || work.ShouldReadRecipients)
        {
            for (var index = 0; index < options.DispatchChunkConcurrency; index++)
            {
                await commandSender.SendAsync(
                    ServiceNames.Notification,
                    new DispatchNotificationBatchV1(command.BatchId),
                    cancellationToken);
            }
        }
    }
}
