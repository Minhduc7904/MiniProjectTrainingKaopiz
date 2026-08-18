// File: backend/Services/Notification/NotificationService.Infrastructure/Persistence/Repositories/EfNotificationRepository.cs
// Mục đích: Lưu Notification trực tiếp và đọc projection theo ID qua EF Core, dùng mapper tại persistence boundary.

using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Repositories;
using NotificationService.Application.Repositories.Models;
using NotificationService.Domain.Constants;
using NotificationService.Infrastructure.Persistence.Context;
using NotificationService.Infrastructure.Persistence.Mappers;
using NotificationService.Infrastructure.Persistence.Scaffolded;

namespace NotificationService.Infrastructure.Persistence.Repositories;

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
        return Task.FromResult(NotificationPersistenceMapper.ToSummary(entity));
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
}
