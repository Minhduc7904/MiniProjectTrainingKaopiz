using System;
using System.Collections.Generic;

namespace NotificationService.Infrastructure.Persistence.Scaffolded;

public partial class NotificationBatch
{
    /// <summary>
    /// UUID định danh yêu cầu gửi notification hàng loạt
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// UUID Course liên quan; null nếu không gửi theo Course
    /// </summary>
    public Guid? CourseId { get; set; }

    /// <summary>
    /// Tiêu đề notification dùng cho toàn batch
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// Nội dung Markdown dùng cho toàn batch; có thể nhúng media
    /// </summary>
    public string BodyMarkdown { get; set; } = null!;

    /// <summary>
    /// COURSE_ENROLLED | STUDENT_IDS | ALL_STUDENTS
    /// </summary>
    public string TargetScope { get; set; } = null!;

    /// <summary>
    /// UUID admin tạo batch; logical reference
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// PENDING | PROCESSING | COMPLETED | PARTIAL_FAILED | FAILED
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Tổng recipient đã snapshot khi tạo batch
    /// </summary>
    public uint TotalCount { get; set; }

    /// <summary>
    /// Số recipient đã được xử lý
    /// </summary>
    public uint ProcessedCount { get; set; }

    /// <summary>
    /// Số notification inbox tạo thành công
    /// </summary>
    public uint SuccessCount { get; set; }

    /// <summary>
    /// Số recipient thất bại sau retry
    /// </summary>
    public uint FailedCount { get; set; }

    /// <summary>
    /// Số item nghiệp vụ xử lý trên mỗi chunk
    /// </summary>
    public uint BatchSize { get; set; }

    /// <summary>
    /// Thời điểm bắt đầu xử lý batch, UTC
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Thời điểm kết thúc batch, UTC
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Thời điểm tạo batch, UTC
    /// </summary>
    public DateTime CreatedAt { get; set; }

    public virtual ICollection<NotificationBatchItem> NotificationBatchItems { get; set; } = new List<NotificationBatchItem>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
