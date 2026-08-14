using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using NotificationService.Application.Abstractions;
using NotificationService.Application.Contracts.Messaging;

namespace NotificationService.Application.Features.Batches.Dispatch;

public sealed class DispatchNotificationBatchHandler(INotificationBatchRepository repository, INotificationSender sender, ICommandSender commandSender)
{
    public async Task HandleAsync(DispatchNotificationBatchV1 command, CancellationToken cancellationToken)
    {
        var items = await repository.ClaimChunkAsync(command.BatchId, cancellationToken);
        foreach (var item in items)
        {
            try
            {
                await sender.SendAsync(item.StudentId, checked((int)item.RetryCount) + 1, cancellationToken);
                await repository.MarkSuccessAsync(item, cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                await repository.MarkFailureAsync(item, "Fake sender failed.", cancellationToken);
            }
        }

        if (await repository.FinalizeOrHasRemainingAsync(command.BatchId, cancellationToken))
        {
            await commandSender.SendAsync(ServiceNames.Notification, command, cancellationToken);
        }
    }
}
