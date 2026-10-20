using System;
using System.Collections.Generic;

namespace CourseService.Infrastructure.Persistence.Scaffolded;

public partial class QuizAttemptAnswer
{
    /// <summary>
    /// UUID định danh đáp án đã chọn
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// UUID lần làm bài
    /// </summary>
    public Guid AttemptId { get; set; }

    /// <summary>
    /// UUID câu hỏi được trả lời
    /// </summary>
    public Guid QuestionId { get; set; }

    /// <summary>
    /// UUID lựa chọn học viên đang chọn
    /// </summary>
    public Guid SelectedOptionId { get; set; }

    public virtual QuizAttempt Attempt { get; set; } = null!;

    public virtual QuizQuestion Question { get; set; } = null!;

    public virtual QuizOption SelectedOption { get; set; } = null!;
}
