// File: backend/Services/Notification/NotificationService.Infrastructure/Persistence/Mappers/NotificationBatchPersistenceMapper.cs
// Mục đích: Map Notification Batch giữa EF entity, Domain state và Application summary mà không trộn layer models.

using NotificationService.Application.Repositories.Models;
using NotificationService.Domain.Entities;
using NotificationBatchEntity = NotificationService.Infrastructure.Persistence.Scaffolded.NotificationBatch;

namespace NotificationService.Infrastructure.Persistence.Mappers;

public static class NotificationBatchPersistenceMapper
{
    public static NotificationBatchState ToDomain(NotificationBatchEntity entity) =>
        new(entity.Status, entity.TotalCount, entity.ProcessedCount, entity.SuccessCount,
            entity.FailedCount, entity.StartedAt, entity.CompletedAt);

    public static void Apply(NotificationBatchState state, NotificationBatchEntity entity)
    {
        entity.Status = state.Status;
        entity.TotalCount = state.TotalCount;
        entity.ProcessedCount = state.ProcessedCount;
        entity.SuccessCount = state.SuccessCount;
        entity.FailedCount = state.FailedCount;
        entity.StartedAt = state.StartedAtUtc;
        entity.CompletedAt = state.CompletedAtUtc;
    }

    public static NotificationBatchSummary ToSummary(NotificationBatchEntity entity) =>
        new(entity.Id, entity.Status, entity.TotalCount, entity.ProcessedCount,
            entity.SuccessCount, entity.FailedCount, entity.BatchSize, entity.CreatedAt,
            entity.StartedAt, entity.CompletedAt);
}
