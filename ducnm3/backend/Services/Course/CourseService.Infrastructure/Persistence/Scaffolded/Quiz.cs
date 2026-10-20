using System;
using System.Collections.Generic;

namespace CourseService.Infrastructure.Persistence.Scaffolded;

public partial class Quiz
{
    /// <summary>
    /// UUID định danh Quiz
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// UUID Lesson sở hữu Quiz
    /// </summary>
    public Guid LessonId { get; set; }

    /// <summary>
    /// Tiêu đề Quiz
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// Mô tả Markdown; có thể nhúng media
    /// </summary>
    public string? DescriptionMarkdown { get; set; }

    /// <summary>
    /// Thứ tự hiển thị Quiz trong Lesson
    /// </summary>
    public uint DisplayOrder { get; set; }

    /// <summary>
    /// DRAFT | PUBLISHED; học viên chỉ thấy PUBLISHED
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Thời điểm tạo Quiz, UTC
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Thời điểm cập nhật gần nhất, UTC
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    public virtual Lesson Lesson { get; set; } = null!;

    public virtual ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();

    public virtual ICollection<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();
}
