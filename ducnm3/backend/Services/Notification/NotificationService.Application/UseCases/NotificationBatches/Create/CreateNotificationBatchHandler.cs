// File: backend/Services/Notification/NotificationService.Application/UseCases/NotificationBatches/Create/CreateNotificationBatchHandler.cs
// Mục đích: Validate batch ALL_STUDENTS, lưu trạng thái PENDING và phát command snapshot recipient qua outbox.

using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using NotificationService.Application.Repositories;
using NotificationService.Application.Repositories.Models;
using NotificationService.Application.Common.Errors;
using NotificationService.Application.Services.Content;
using NotificationService.Application.Contracts.Messaging;
using NotificationService.Domain.Constants;
using MediaService.Contracts.Messaging;

namespace NotificationService.Application.UseCases.NotificationBatches.Create;

public sealed class CreateNotificationBatchHandler(
    INotificationBatchRepository repository,
    ICommandSender commandSender,
    NotificationMediaReferenceExtractor mediaReferenceExtractor,
    TimeProvider timeProvider)
{
    public async Task<NotificationBatchSummary> HandleAsync(CreateNotificationBatchCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (!string.Equals(command.TargetScope?.Trim(), NotificationTargetScopes.AllStudents, StringComparison.Ordinal) || command.CourseId is not null || command.CreatedBy == Guid.Empty || string.IsNullOrWhiteSpace(command.Title) || command.Title.Length > 200 || string.IsNullOrWhiteSpace(command.BodyMarkdown))
        {
            throw NotificationErrors.Validation("Only ALL_STUDENTS, title, bodyMarkdown and createdBy are accepted.");
        }

        var batchSize = command.BatchSize ?? 500;
        if (batchSize is < 1 or > 1000)
        {
            throw NotificationErrors.Validation("batchSize must be between 1 and 1000.");
        }

        if (command.RequestedCount is 0 or > 100000)
        {
            throw NotificationErrors.Validation("requestedCount must be null or between 1 and 100000.");
        }

        _ = mediaReferenceExtractor.Extract(command.BodyMarkdown);
        var result = await repository.CreateAsync(new CreateNotificationBatchRecord(
            Guid.NewGuid(), command.Title.Trim(), command.BodyMarkdown,
            NotificationTargetScopes.AllStudents, command.CreatedBy, batchSize,
            command.RequestedCount, null, timeProvider.GetUtcNow().UtcDateTime), cancellationToken);
        await commandSender.SendAsync(ServiceNames.Notification, new SnapshotNotificationBatchV1(result.Id), cancellationToken);
        await commandSender.SendAsync(
            ServiceNames.Media,
            new StartNotificationMediaUsageJobV1(result.Id),
            cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return result;
    }
}
