// File: backend/Services/Media/MediaService.Infrastructure/Persistence/Context/MediaDbContext.Outbox.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

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
