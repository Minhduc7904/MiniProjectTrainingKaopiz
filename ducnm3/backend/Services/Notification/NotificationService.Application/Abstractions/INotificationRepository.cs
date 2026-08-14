namespace NotificationService.Application.Abstractions;

public interface INotificationRepository
{
    Task<NotificationSummary> CreateAsync(
        CreateNotificationRecord record,
        CancellationToken cancellationToken);

    Task<NotificationSummary?> GetByIdAsync(
        Guid notificationId,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public sealed record CreateNotificationRecord(
    Guid Id,
    Guid RecipientStudentId,
    string Title,
    string BodyMarkdown,
    Guid CreatedBy,
    DateTime CreatedAtUtc);

public sealed record NotificationSummary(
    Guid Id,
    Guid RecipientStudentId,
    string Title,
    string BodyMarkdown,
    string SourceType,
    string Status,
    Guid CreatedBy,
    DateTime CreatedAtUtc,
    DateTime? ReadAtUtc);
