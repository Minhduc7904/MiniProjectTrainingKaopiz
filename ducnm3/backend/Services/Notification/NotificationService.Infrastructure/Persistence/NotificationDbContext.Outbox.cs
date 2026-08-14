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
