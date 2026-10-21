using System;
using System.Collections.Generic;

namespace CourseService.Infrastructure.Persistence.Scaffolded;

public partial class LessonAiJob
{
    /// <summary>
    /// UUID định danh job AI
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// UUID Lesson mà job ghi kết quả vào
    /// </summary>
    public Guid LessonId { get; set; }

    /// <summary>
    /// UUID Course của Lesson, dùng khi gọi handler cập nhật Lesson
    /// </summary>
    public Guid CourseId { get; set; }

    /// <summary>
    /// SYNTHESIZE_LESSON | GENERATE_QUIZ
    /// </summary>
    public string JobType { get; set; } = null!;

    /// <summary>
    /// PENDING | RUNNING | COMPLETED | FAILED
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Mảng UUID media (tài liệu) mà admin đã chọn làm nguồn
    /// </summary>
    public string MediaIdsJson { get; set; } = null!;

    /// <summary>
    /// Số câu hỏi yêu cầu khi GENERATE_QUIZ; NULL với SYNTHESIZE_LESSON
    /// </summary>
    public int? QuestionCount { get; set; }

    /// <summary>
    /// Admin đã xác nhận cho ghi đè nội dung Lesson đang có
    /// </summary>
    public bool OverwriteContent { get; set; }

    /// <summary>
    /// Nội dung Lesson trước khi job ghi đè, để khôi phục
    /// </summary>
    public string? PreviousContentMarkdown { get; set; }

    /// <summary>
    /// UUID Quiz DRAFT do job tạo; chỉ có khi GENERATE_QUIZ hoàn tất
    /// </summary>
    public Guid? ResultQuizId { get; set; }

    /// <summary>
    /// Số lượt gọi model của job
    /// </summary>
    public int StepCount { get; set; }

    /// <summary>
    /// Mã lỗi an toàn khi FAILED
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Thông báo lỗi an toàn hiển thị cho admin
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Hạn lease của worker đang chạy job, UTC; quá hạn thì worker khác nhận lại
    /// </summary>
    public DateTime? LockedUntil { get; set; }

    /// <summary>
    /// UUID admin tạo job; dùng làm actor khi gọi AI Service và ghi dữ liệu
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Thời điểm tạo job, UTC
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Thời điểm cập nhật gần nhất, UTC
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Thời điểm worker bắt đầu chạy, UTC
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Thời điểm job kết thúc (COMPLETED hoặc FAILED), UTC
    /// </summary>
    public DateTime? FinishedAt { get; set; }

    public virtual Lesson Lesson { get; set; } = null!;
}
