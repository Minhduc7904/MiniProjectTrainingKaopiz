// File: backend/Services/Media/MediaService.Infrastructure/Persistence/Scaffolded/NotificationMediaUsageJob.cs
// Mục đích: Phản ánh bảng notification_media_usage_jobs được scaffold từ migration V006 cho EF Core.

namespace MediaService.Infrastructure.Persistence.Scaffolded;

public partial class NotificationMediaUsageJob
{
    public Guid Id { get; set; }
    public string Status { get; set; } = null!;
    public uint? ExpectedUsageCount { get; set; }
    public uint ProcessedUsageCount { get; set; }
    public uint FailedUsageCount { get; set; }
    public string? LastError { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
