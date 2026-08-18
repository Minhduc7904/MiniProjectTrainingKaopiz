// File: backend/Services/Notification/NotificationService.Infrastructure/Persistence/Repositories/EfNotificationRepository.cs
// Mục đích: Triển khai repository EfNotificationRepository bằng EF Core và persistence model.

using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Abstractions;
using NotificationService.Domain.Notifications;
using NotificationService.Infrastructure.Persistence.Scaffolded;

namespace NotificationService.Infrastructure.Persistence;

public sealed class EfNotificationRepository(NotificationDbContext dbContext)
    : INotificationRepository
{
    public Task<NotificationSummary> CreateAsync(
        CreateNotificationRecord record,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var entity = new Notification
        {
            Id = record.Id,
            RecipientStudentId = record.RecipientStudentId,
            Title = record.Title,
            BodyMarkdown = record.BodyMarkdown,
            SourceType = NotificationSourceTypes.Direct,
            Status = NotificationStatuses.Unread,
            CreatedBy = record.CreatedBy,
            CreatedAt = record.CreatedAtUtc,
        };
        dbContext.Notifications.Add(entity);
        return Task.FromResult(ToSummary(entity));
    }

    public async Task<NotificationSummary?> GetByIdAsync(
        Guid notificationId,
        CancellationToken cancellationToken) =>
        await dbContext.Notifications
            .AsNoTracking()
            .Where(item => item.Id == notificationId)
            .Select(item => new NotificationSummary(
                item.Id,
                item.RecipientStudentId,
                item.Title,
                item.BodyMarkdown,
                item.SourceType,
                item.Status,
                item.CreatedBy,
                item.CreatedAt,
                item.ReadAt))
            .SingleOrDefaultAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);

    private static NotificationSummary ToSummary(Notification entity) =>
        new(
            entity.Id,
            entity.RecipientStudentId,
            entity.Title,
            entity.BodyMarkdown,
            entity.SourceType,
            entity.Status,
            entity.CreatedBy,
            entity.CreatedAt,
            entity.ReadAt);
}
