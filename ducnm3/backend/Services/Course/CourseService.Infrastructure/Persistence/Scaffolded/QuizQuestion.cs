using System;
using System.Collections.Generic;

namespace CourseService.Infrastructure.Persistence.Scaffolded;

public partial class QuizQuestion
{
    /// <summary>
    /// UUID định danh câu hỏi
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// UUID Quiz sở hữu câu hỏi
    /// </summary>
    public Guid QuizId { get; set; }

    /// <summary>
    /// Nội dung câu hỏi Markdown; có thể nhúng media và công thức
    /// </summary>
    public string QuestionMarkdown { get; set; } = null!;

    /// <summary>
    /// SINGLE_CHOICE | MULTIPLE_CHOICE
    /// </summary>
    public string QuestionType { get; set; } = null!;

    /// <summary>
    /// Điểm của câu hỏi, lớn hơn 0 và tối đa 100
    /// </summary>
    public decimal Points { get; set; }

    /// <summary>
    /// Thứ tự hiển thị câu hỏi trong Quiz
    /// </summary>
    public uint DisplayOrder { get; set; }

    public virtual Quiz Quiz { get; set; } = null!;

    public virtual ICollection<QuizAttemptAnswer> QuizAttemptAnswers { get; set; } = new List<QuizAttemptAnswer>();

    public virtual ICollection<QuizOption> QuizOptions { get; set; } = new List<QuizOption>();
}
