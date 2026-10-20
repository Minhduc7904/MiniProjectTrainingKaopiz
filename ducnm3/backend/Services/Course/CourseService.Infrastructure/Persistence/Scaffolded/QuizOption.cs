using System;
using System.Collections.Generic;

namespace CourseService.Infrastructure.Persistence.Scaffolded;

public partial class QuizOption
{
    /// <summary>
    /// UUID định danh lựa chọn
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// UUID câu hỏi sở hữu lựa chọn
    /// </summary>
    public Guid QuestionId { get; set; }

    /// <summary>
    /// Nội dung lựa chọn Markdown; có thể nhúng media và công thức
    /// </summary>
    public string OptionMarkdown { get; set; } = null!;

    /// <summary>
    /// Lựa chọn này có phải đáp án đúng hay không
    /// </summary>
    public bool IsCorrect { get; set; }

    /// <summary>
    /// Thứ tự hiển thị lựa chọn trong câu hỏi
    /// </summary>
    public uint DisplayOrder { get; set; }

    public virtual QuizQuestion Question { get; set; } = null!;

    public virtual ICollection<QuizAttemptAnswer> QuizAttemptAnswers { get; set; } = new List<QuizAttemptAnswer>();
}
