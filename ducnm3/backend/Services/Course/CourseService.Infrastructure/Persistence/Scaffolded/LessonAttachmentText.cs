using System;
using System.Collections.Generic;

namespace CourseService.Infrastructure.Persistence.Scaffolded;

public partial class LessonAttachmentText
{
    /// <summary>
    /// UUID định danh bản trích xuất
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// UUID Lesson sở hữu tài liệu
    /// </summary>
    public Guid LessonId { get; set; }

    /// <summary>
    /// UUID media đính kèm từ Media Service; logical reference
    /// </summary>
    public Guid MediaId { get; set; }

    /// <summary>
    /// Tên tệp gốc tại lúc đưa vào hàng chờ, để hiển thị
    /// </summary>
    public string FileName { get; set; } = null!;

    /// <summary>
    /// MIME type của tệp tại lúc đưa vào hàng chờ
    /// </summary>
    public string ContentType { get; set; } = null!;

    /// <summary>
    /// PENDING | EXTRACTING | EXTRACTED | FAILED | UNSUPPORTED
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Văn bản (Markdown) đã trích xuất; chỉ có khi EXTRACTED
    /// </summary>
    public string? ExtractedText { get; set; }

    /// <summary>
    /// Mã lỗi an toàn khi FAILED hoặc UNSUPPORTED
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Thông báo lỗi an toàn hiển thị cho admin
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Số lần worker đã nhận trích xuất tài liệu này
    /// </summary>
    public int Attempts { get; set; }

    /// <summary>
    /// Hạn lease của worker đang trích xuất, UTC; quá hạn thì worker khác nhận lại
    /// </summary>
    public DateTime? LockedUntil { get; set; }

    /// <summary>
    /// UUID admin đã yêu cầu; dùng làm actor khi gọi AI Service
    /// </summary>
    public Guid RequestedBy { get; set; }

    /// <summary>
    /// Thời điểm vào hàng chờ, UTC
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Thời điểm cập nhật gần nhất, UTC
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    public virtual Lesson Lesson { get; set; } = null!;
}
