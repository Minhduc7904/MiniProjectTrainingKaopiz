using System;
using System.Collections.Generic;

namespace NotificationService.Infrastructure.Persistence.Scaffolded;

public partial class NotificationJob
{
    /// <summary>
    /// UUID định danh batch job
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// UUID Course liên quan; null nếu không gửi theo Course
    /// </summary>
    public Guid? CourseId { get; set; }

    /// <summary>
    /// Tiêu đề thông báo
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// Nội dung Markdown; có thể nhúng media
    /// </summary>
    public string BodyMarkdown { get; set; } = null!;

    /// <summary>
    /// COURSE_ENROLLED | STUDENT_IDS | ALL_STUDENTS
    /// </summary>
    public string TargetScope { get; set; } = null!;

    /// <summary>
    /// UUID admin tạo job; logical reference
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// PENDING | PROCESSING | COMPLETED | PARTIAL_FAILED | FAILED
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Tổng recipient đã snapshot khi tạo job
    /// </summary>
    public uint TotalCount { get; set; }

    /// <summary>
    /// Số recipient worker đã xử lý
    /// </summary>
    public uint ProcessedCount { get; set; }

    /// <summary>
    /// Số notification tạo thành công
    /// </summary>
    public uint SuccessCount { get; set; }

    /// <summary>
    /// Số recipient thất bại sau retry
    /// </summary>
    public uint FailedCount { get; set; }

    /// <summary>
    /// Số item xử lý trên mỗi chunk
    /// </summary>
    public uint BatchSize { get; set; }

    /// <summary>
    /// Thời điểm worker bắt đầu; null khi job chưa chạy
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Thời điểm job kết thúc; null khi chưa hoàn tất
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Thời điểm tạo job, UTC
    /// </summary>
    public DateTime CreatedAt { get; set; }

    public virtual ICollection<NotificationJobItem> NotificationJobItems { get; set; } = new List<NotificationJobItem>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
