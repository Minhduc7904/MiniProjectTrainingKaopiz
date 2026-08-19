using System;
using System.Collections.Generic;

namespace SchedulerService.Infrastructure.Persistence.Scaffolded;

public partial class BackgroundJobRun
{
    /// <summary>
    /// UUID định danh một lần thực thi job
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// UUID background_jobs.id trong Scheduler database
    /// </summary>
    public Guid BackgroundJobId { get; set; }

    /// <summary>
    /// MANUAL | CRON | RETRY
    /// </summary>
    public string TriggerType { get; set; } = null!;

    /// <summary>
    /// Khóa chống tạo trùng run cho cùng job
    /// </summary>
    public string IdempotencyKey { get; set; } = null!;

    /// <summary>
    /// Snapshot payload tại thời điểm tạo run để phục vụ audit
    /// </summary>
    public string? PayloadSnapshotJson { get; set; }

    /// <summary>
    /// Lần thử hiện tại, bắt đầu từ 1
    /// </summary>
    public uint AttemptNumber { get; set; }

    /// <summary>
    /// QUEUED | RUNNING | SUCCEEDED | FAILED | CANCELLED | TIMED_OUT | SKIPPED
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Thời điểm UTC run được lên lịch hoặc manual trigger
    /// </summary>
    public DateTime ScheduledAt { get; set; }

    /// <summary>
    /// Thời điểm worker bắt đầu xử lý, UTC
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Thời điểm worker kết thúc xử lý, UTC
    /// </summary>
    public DateTime? FinishedAt { get; set; }

    /// <summary>
    /// Định danh worker instance thực thi run
    /// </summary>
    public string? WorkerInstance { get; set; }

    /// <summary>
    /// Correlation ID dùng nối log giữa Scheduler và target service
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Mã lỗi ổn định cuối cùng; null khi chưa lỗi
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Thông tin lỗi an toàn cho vận hành; không chứa credential
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Kết quả tóm tắt của run; không dùng thay domain database
    /// </summary>
    public string? OutputJson { get; set; }

    /// <summary>
    /// Thời điểm tạo run, UTC
    /// </summary>
    public DateTime CreatedAt { get; set; }

    public virtual BackgroundJob BackgroundJob { get; set; } = null!;
}
