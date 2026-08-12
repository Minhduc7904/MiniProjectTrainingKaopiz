using System;
using System.Collections.Generic;

namespace CourseService.Infrastructure.Persistence.Scaffolded;

public partial class Lesson
{
    /// <summary>
    /// UUID định danh Lesson
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// UUID Course sở hữu Lesson
    /// </summary>
    public Guid CourseId { get; set; }

    /// <summary>
    /// Tiêu đề Lesson
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// Thứ tự hiển thị Lesson trong Course
    /// </summary>
    public uint DisplayOrder { get; set; }

    /// <summary>
    /// Nội dung Markdown; có thể nhúng media
    /// </summary>
    public string? ContentMarkdown { get; set; }

    /// <summary>
    /// Thời điểm tạo Lesson, UTC
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Thời điểm cập nhật gần nhất, UTC
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
}
