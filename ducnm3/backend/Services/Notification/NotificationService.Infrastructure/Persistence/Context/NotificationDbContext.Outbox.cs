// File: backend/Services/Notification/NotificationService.Infrastructure/Persistence/Context/NotificationDbContext.Outbox.cs
// Mục đích: Mở rộng NotificationDbContext với OutboxMessages để lưu message cùng transaction dữ liệu nghiệp vụ.

using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace NotificationService.Infrastructure.Persistence.Context;

public partial class NotificationDbContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        _ = GetType();
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
