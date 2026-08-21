// File: backend/Services/Media/MediaService.Infrastructure/Persistence/Scaffolded/MediaBackgroundJob.cs
// Mục đích: Phản ánh bảng media_background_jobs được scaffold từ migration V009.

namespace MediaService.Infrastructure.Persistence.Scaffolded;

public partial class MediaBackgroundJob
{
    public Guid Id { get; set; }
    public string JobType { get; set; } = null!;
    public string SubjectType { get; set; } = null!;
    public Guid SubjectId { get; set; }
    public Guid? CorrelationId { get; set; }
    public string? DeduplicationKey { get; set; }
    public string Status { get; set; } = null!;
    public string PayloadJson { get; set; } = null!;
    public uint? ExpectedItemCount { get; set; }
    public uint ProcessedItemCount { get; set; }
    public uint FailedItemCount { get; set; }
    public uint AttemptCount { get; set; }
    public string? LastError { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
