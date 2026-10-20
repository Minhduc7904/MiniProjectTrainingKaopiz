using System;
using System.Collections.Generic;

namespace CourseService.Infrastructure.Persistence.Scaffolded;

public partial class QuizAttempt
{
    /// <summary>
    /// UUID định danh lần làm bài
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// UUID Quiz được làm
    /// </summary>
    public Guid QuizId { get; set; }

    /// <summary>
    /// UUID Student từ Student Service; logical reference
    /// </summary>
    public Guid StudentId { get; set; }

    /// <summary>
    /// IN_PROGRESS | SUBMITTED
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Thời điểm bắt đầu làm bài, UTC
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Thời điểm lưu nháp hoặc nộp gần nhất, UTC
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Thời điểm nộp bài; null khi đang làm dở
    /// </summary>
    public DateTime? SubmittedAt { get; set; }

    /// <summary>
    /// Điểm đạt được; chỉ có khi đã nộp
    /// </summary>
    public decimal? Score { get; set; }

    /// <summary>
    /// Tổng điểm tối đa tại lúc nộp; chỉ có khi đã nộp
    /// </summary>
    public decimal? MaxScore { get; set; }

    /// <summary>
    /// Chỉ có giá trị khi IN_PROGRESS; unique để mỗi Student có tối đa một bài làm dở cho mỗi Quiz
    /// </summary>
    public Guid? OpenMarker { get; set; }

    public virtual Quiz Quiz { get; set; } = null!;

    public virtual ICollection<QuizAttemptAnswer> QuizAttemptAnswers { get; set; } = new List<QuizAttemptAnswer>();
}
