using System;
using System.Collections.Generic;

namespace CourseService.Infrastructure.Persistence.Scaffolded;

public partial class Course
{
    /// <summary>
    /// UUID định danh Course
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Tên Course hiển thị cho người dùng
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Nội dung mô tả Markdown; có thể nhúng media
    /// </summary>
    public string? DescriptionMarkdown { get; set; }

    /// <summary>
    /// DRAFT | PUBLISHED | ARCHIVED
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Thời điểm tạo Course, UTC
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Thời điểm cập nhật gần nhất, UTC
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
