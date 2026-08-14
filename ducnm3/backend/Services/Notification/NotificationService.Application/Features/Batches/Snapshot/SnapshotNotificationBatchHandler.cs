using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using NotificationService.Application.Abstractions;
using NotificationService.Application.Features.Batches;
using NotificationService.Application.Contracts.Messaging;

namespace NotificationService.Application.Features.Batches.Snapshot;

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
