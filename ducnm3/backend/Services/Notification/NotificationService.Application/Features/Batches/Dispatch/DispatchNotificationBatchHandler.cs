using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using MediaService.Contracts.Messaging;
using NotificationService.Application.Abstractions;
using NotificationService.Application.Content;
using NotificationService.Application.Contracts.Messaging;

namespace NotificationService.Application.Features.Batches.Dispatch;

public sealed class DispatchNotificationBatchHandler(
    INotificationBatchRepository repository,
    INotificationSender sender,
    ICommandSender commandSender,
    NotificationMediaReferenceExtractor mediaReferenceExtractor)
{
    public async Task HandleAsync(DispatchNotificationBatchV1 command, CancellationToken cancellationToken)
    {
        var items = await repository.ClaimChunkAsync(command.BatchId, cancellationToken);
        foreach (var item in items)
        {
            try
            {
                await sender.SendAsync(item.StudentId, checked((int)item.RetryCount) + 1, cancellationToken);
                var notification = await repository.MarkSuccessAsync(item, cancellationToken);
                if (notification is not null)
                {
                    var references = mediaReferenceExtractor.Extract(notification.BodyMarkdown);
                    if (references.Count > 0)
                    {
                        await commandSender.SendAsync(
                            ServiceNames.Media,
                            new RegisterNotificationMediaUsageV1(
                                notification.Id,
                                notification.CreatedBy,
                                references),
                            cancellationToken);
                    }
                }
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
