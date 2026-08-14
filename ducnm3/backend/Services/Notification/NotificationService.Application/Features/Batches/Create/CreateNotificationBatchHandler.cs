using BuildingBlocks.Contracts.Api;
using BuildingBlocks.Messaging.Abstractions;
using NotificationService.Application.Abstractions;
using NotificationService.Application.Contracts.Messaging;

namespace NotificationService.Application.Features.Batches.Create;

public sealed class CreateNotificationBatchHandler(
    INotificationBatchRepository repository,
    ICommandSender commandSender,
    TimeProvider timeProvider)
{
    public async Task<NotificationBatchSummary> HandleAsync(CreateNotificationBatchCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (!string.Equals(command.TargetScope?.Trim(), "ALL_STUDENTS", StringComparison.Ordinal) || command.CourseId is not null || command.CreatedBy == Guid.Empty || string.IsNullOrWhiteSpace(command.Title) || command.Title.Length > 200 || string.IsNullOrWhiteSpace(command.BodyMarkdown))
        {
            throw NotificationErrors.Validation("Only ALL_STUDENTS, title, bodyMarkdown and createdBy are accepted.");
        }

        var batchSize = command.BatchSize ?? 500;
        if (batchSize is < 1 or > 1000)
        {
            throw NotificationErrors.Validation("batchSize must be between 1 and 1000.");
        }

        var result = await repository.CreateAsync(new CreateNotificationBatchRecord(Guid.NewGuid(), command.Title.Trim(), command.BodyMarkdown, command.CreatedBy, batchSize, timeProvider.GetUtcNow().UtcDateTime), cancellationToken);
        await commandSender.SendAsync(ServiceNames.Notification, new SnapshotNotificationBatchV1(result.Id), cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return result;
    }
}
