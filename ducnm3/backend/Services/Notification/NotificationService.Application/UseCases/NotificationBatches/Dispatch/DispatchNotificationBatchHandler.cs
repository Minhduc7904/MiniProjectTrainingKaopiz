// File: backend/Services/Notification/NotificationService.Application/UseCases/NotificationBatches/Dispatch/DispatchNotificationBatchHandler.cs
// Mục đích: Claim một chunk có lease, gọi sender từng recipient, hoàn tất kết quả và phát command cho phần việc còn lại.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using MediaService.Contracts.Messaging;
using NotificationService.Application.Repositories;
using NotificationService.Application.Repositories.Models;
using NotificationService.Application.Services.Sending;
using NotificationService.Application.UseCases.NotificationBatches;
using NotificationService.Application.Services.Content;
using NotificationService.Application.Contracts.Messaging;
using System.Collections.Concurrent;

namespace NotificationService.Application.UseCases.NotificationBatches.Dispatch;

public sealed class DispatchNotificationBatchHandler(
    INotificationBatchDispatchRepository repository,
    INotificationSender sender,
    ICommandSender commandSender,
    NotificationMediaReferenceExtractor mediaReferenceExtractor,
    NotificationBatchProcessingOptions options)
{
    public async Task HandleAsync(DispatchNotificationBatchV1 command, CancellationToken cancellationToken)
    {
        var claim = await repository.ClaimChunkAsync(command.BatchId, cancellationToken);
        if (claim is null || claim.Items.Count == 0)
        {
            return;
        }

        var results = new ConcurrentBag<NotificationBatchDeliveryResult>();
        await Parallel.ForEachAsync(
            claim.Items,
            new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = options.MaxConcurrentSends,
            },
            async (item, token) =>
            {
                try
                {
                    await sender.SendAsync(item.StudentId, checked((int)item.RetryCount) + 1, token);
                    results.Add(new NotificationBatchDeliveryResult(item.Id, true, null));
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    results.Add(new NotificationBatchDeliveryResult(item.Id, false, "Fake sender failed."));
                }
            });

        var successfulNotifications = await repository.CompleteClaimAsync(
            claim,
            results.ToArray(),
            cancellationToken);

        if (successfulNotifications.Count > 0)
        {
            var references = mediaReferenceExtractor.Extract(
                successfulNotifications[0].BodyMarkdown);
            if (references.Count > 0)
            {
                var notificationIdsPerCommand = Math.Min(
                    NotificationMediaUsageBatchLimits.MaxNotificationIdsPerCommand,
                    Math.Max(
                        1,
                        NotificationMediaUsageBatchLimits.MaxUsageRowsPerCommand /
                        references.Count));
                foreach (var notificationIds in successfulNotifications
                    .Select(notification => notification.Id)
                    .Distinct()
                    .Chunk(notificationIdsPerCommand))
                {
                    await commandSender.SendAsync(
                        ServiceNames.Media,
                        new RegisterNotificationMediaUsageBatchV1(
                            notificationIds,
                            successfulNotifications[0].CreatedBy,
                            references),
                        cancellationToken);
                }
            }
        }

        if (await repository.FinalizeOrHasRemainingAsync(command.BatchId, cancellationToken))
        {
            await commandSender.SendAsync(ServiceNames.Notification, command, cancellationToken);
        }
    }
}
