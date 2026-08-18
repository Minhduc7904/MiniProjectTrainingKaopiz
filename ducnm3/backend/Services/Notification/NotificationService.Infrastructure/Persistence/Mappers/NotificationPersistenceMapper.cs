// File: backend/Services/Notification/NotificationService.Infrastructure/Persistence/Mappers/NotificationPersistenceMapper.cs
// Mục đích: Map EF Notification entity sang Application summary tại persistence boundary.

using NotificationService.Application.Repositories.Models;
using NotificationEntity = NotificationService.Infrastructure.Persistence.Scaffolded.Notification;

namespace NotificationService.Infrastructure.Persistence.Mappers;

public static class NotificationPersistenceMapper
{
    public static NotificationSummary ToSummary(NotificationEntity entity) =>
        new(entity.Id, entity.RecipientStudentId, entity.Title, entity.BodyMarkdown,
            entity.SourceType, entity.Status, entity.CreatedBy, entity.CreatedAt, entity.ReadAt);
}
