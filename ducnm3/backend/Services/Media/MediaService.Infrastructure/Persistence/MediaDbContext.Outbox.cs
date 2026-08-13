using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace MediaService.Infrastructure.Persistence;

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
