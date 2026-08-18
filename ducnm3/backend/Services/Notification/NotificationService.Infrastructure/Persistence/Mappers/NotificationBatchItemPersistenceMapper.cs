// File: backend/Services/Notification/NotificationService.Infrastructure/Persistence/Mappers/NotificationBatchItemPersistenceMapper.cs
// Mục đích: Map EF Batch Item sang Domain state và áp dụng kết quả transition trở lại row đang được tracking.

using NotificationService.Domain.Entities;
using NotificationBatchItemEntity = NotificationService.Infrastructure.Persistence.Scaffolded.NotificationBatchItem;

namespace NotificationService.Infrastructure.Persistence.Mappers;

public static class NotificationBatchItemPersistenceMapper
{
    public static NotificationBatchItemState ToDomain(NotificationBatchItemEntity entity) =>
        new(entity.Status, entity.RetryCount);

    public static void Apply(NotificationBatchItemState state, NotificationBatchItemEntity entity)
    {
        entity.Status = state.Status;
        entity.RetryCount = state.RetryCount;
        entity.ErrorMessage = state.ErrorMessage;
        entity.ProcessedAt = state.ProcessedAtUtc;
    }
}
