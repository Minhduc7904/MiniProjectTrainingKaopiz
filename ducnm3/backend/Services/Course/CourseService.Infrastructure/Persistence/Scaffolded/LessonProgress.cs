using System;
using System.Collections.Generic;

namespace CourseService.Infrastructure.Persistence.Scaffolded;

public partial class LessonProgress
{
    /// <summary>
    /// UUID định danh tiến độ học
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// UUID Lesson được theo dõi tiến độ
    /// </summary>
    public Guid LessonId { get; set; }

    /// <summary>
    /// UUID Student từ Student Service; logical reference
    /// </summary>
    public Guid StudentId { get; set; }

    /// <summary>
    /// Phần trăm hoàn thành, từ 0 đến 100
    /// </summary>
    public decimal ProgressPercent { get; set; }

    /// <summary>
    /// Thời điểm hoàn thành; null khi chưa hoàn thành
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Thời điểm cập nhật tiến độ gần nhất, UTC
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    public virtual Lesson Lesson { get; set; } = null!;
}
