// File: backend/Services/Notification/NotificationService.Infrastructure/Persistence/Context/NotificationDbContext.Outbox.cs
// Mục đích: Cấu hình EF Core DbContext và mapping database cho service.

using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace NotificationService.Infrastructure.Persistence;

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
