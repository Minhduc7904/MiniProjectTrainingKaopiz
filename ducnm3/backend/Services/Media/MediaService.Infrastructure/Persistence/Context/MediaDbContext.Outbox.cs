// File: backend/Services/Media/MediaService.Infrastructure/Persistence/Context/MediaDbContext.Outbox.cs
// Mục đích: Cấu hình EF Core DbContext và mapping database cho service.

using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace MediaService.Infrastructure.Persistence.Context;

public partial class MediaDbContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        _ = GetType();
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
