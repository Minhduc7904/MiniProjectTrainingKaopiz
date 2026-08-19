using System;
using System.Collections.Generic;

namespace CourseService.Infrastructure.Persistence.Scaffolded;

public partial class Enrollment
{
    /// <summary>
    /// UUID định danh lượt ghi danh
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// UUID Course được ghi danh
    /// </summary>
    public Guid CourseId { get; set; }

    /// <summary>
    /// UUID Student từ Student Service; logical reference
    /// </summary>
    public Guid StudentId { get; set; }

    /// <summary>
    /// Thời điểm Student ghi danh, UTC
    /// </summary>
    public DateTime EnrolledAt { get; set; }

    public virtual Course Course { get; set; } = null!;
}
