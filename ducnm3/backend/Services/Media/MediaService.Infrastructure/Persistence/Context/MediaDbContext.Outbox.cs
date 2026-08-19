// File: backend/Services/Media/MediaService.Infrastructure/Persistence/Context/MediaDbContext.Outbox.cs
// Mục đích: Cấu hình EF Core DbContext và mapping database cho service.

using MassTransit;
using Microsoft.EntityFrameworkCore;
using MediaService.Infrastructure.Persistence.Scaffolded;

namespace MediaService.Infrastructure.Persistence.Context;

public partial class MediaDbContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        _ = GetType();
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        // Database default của is_draft là true. Sentinel true bảo đảm false
        // được gửi explicit trong INSERT thay vì bị database default ghi đè.
        modelBuilder.Entity<MediaObject>()
            .Property(media => media.IsDraft)
            .HasSentinel(true);
    }
}
