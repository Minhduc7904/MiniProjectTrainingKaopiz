using System;
using System.Collections.Generic;

namespace SchedulerService.Infrastructure.Persistence.Scaffolded;

public partial class BackgroundJob
{
    /// <summary>
    /// UUID định danh cấu hình background job
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Khóa ổn định, duy nhất dùng để đăng ký và tra cứu job
    /// </summary>
    public string JobKey { get; set; } = null!;

    /// <summary>
    /// Tên job hiển thị cho vận hành
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Loại handler tương lai, ví dụ NOTIFICATION_BATCH_DISPATCH hoặc MEDIA_UNUSED_CLEANUP
    /// </summary>
    public string JobType { get; set; } = null!;

    /// <summary>
    /// Service sở hữu nghiệp vụ sẽ được gọi trong phase execution tương lai
    /// </summary>
    public string TargetService { get; set; } = null!;

    /// <summary>
    /// MANUAL | CRON
    /// </summary>
    public string ScheduleType { get; set; } = null!;

    /// <summary>
    /// Biểu thức CRON theo UTC; null với MANUAL
    /// </summary>
    public string? CronExpression { get; set; }

    /// <summary>
    /// Cấu hình đầu vào chung; không lưu recipient list hoặc dữ liệu domain lớn
    /// </summary>
    public string? PayloadJson { get; set; }

    /// <summary>
    /// ACTIVE | PAUSED | DISABLED
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Cho phép nhiều run đồng thời của cùng job hay không
    /// </summary>
    public bool AllowConcurrent { get; set; }

    /// <summary>
    /// Số lần retry tối đa dành cho execution phase tương lai
    /// </summary>
    public uint MaxRetryCount { get; set; }

    /// <summary>
    /// Thời gian chạy tối đa trước khi đánh dấu timeout
    /// </summary>
    public uint TimeoutSeconds { get; set; }

    /// <summary>
    /// Thời điểm UTC chạy CRON kế tiếp; null khi chưa tính hoặc là MANUAL
    /// </summary>
    public DateTime? NextRunAt { get; set; }

    /// <summary>
    /// UUID actor tạo job; null với system-defined job
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// Thời điểm tạo job, UTC
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Thời điểm cập nhật job gần nhất, UTC
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<BackgroundJobRun> BackgroundJobRuns { get; set; } = new List<BackgroundJobRun>();
}
