// File: backend/Services/Media/MediaService.Infrastructure/Persistence/Scaffolded/MediaObject.cs
// Mục đích: Entity EF scaffolded MediaObject ánh xạ bảng database hiện có; phục vụ persistence và không chứa nghiệp vụ use case.

using System;
using System.Collections.Generic;

namespace MediaService.Infrastructure.Persistence.Scaffolded;

public partial class MediaObject
{
    /// <summary>
    /// UUID định danh media
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Media gốc của object dẫn xuất; null với file upload gốc
    /// </summary>
    public Guid? SourceMediaId { get; set; }

    /// <summary>
    /// THUMBNAIL với object dẫn xuất; null với file upload gốc
    /// </summary>
    public string? DerivationType { get; set; }

    /// <summary>
    /// Tên bucket MinIO chứa object
    /// </summary>
    public string Bucket { get; set; } = null!;

    /// <summary>
    /// Khóa object duy nhất trong bucket; không trả trực tiếp cho client
    /// </summary>
    public string ObjectKey { get; set; } = null!;

    /// <summary>
    /// IMAGE | VIDEO | DOCUMENT | AUDIO | OTHER
    /// </summary>
    public string MediaType { get; set; } = null!;

    /// <summary>
    /// MIME type đã xác thực
    /// </summary>
    public string ContentType { get; set; } = null!;

    /// <summary>
    /// Tên file do người dùng upload, chỉ để hiển thị
    /// </summary>
    public string OriginalFileName { get; set; } = null!;

    /// <summary>
    /// Kích thước object theo byte
    /// </summary>
    public ulong SizeBytes { get; set; }

    /// <summary>
    /// Hash SHA-256 kiểm tra toàn vẹn; null khi upload chưa READY
    /// </summary>
    public string? ChecksumSha256 { get; set; }

    /// <summary>
    /// UUID user/admin upload media; logical reference
    /// </summary>
    public Guid UploadedBy { get; set; }

    /// <summary>
    /// Actor type thực hiện upload; được Application validate
    /// </summary>
    public string UploadedByType { get; set; } = null!;

    /// <summary>
    /// PENDING | READY | FAILED
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Media chưa được gắn với usage active
    /// </summary>
    public bool IsDraft { get; set; }

    /// <summary>
    /// Lỗi an toàn nội bộ khi upload FAILED; không trả cho client
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// Thời điểm upload chuyển READY, UTC
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Thời điểm media bắt đầu ở trạng thái draft, UTC
    /// </summary>
    public DateTime? DraftedAt { get; set; }

    /// <summary>
    /// Thời điểm media record cập nhật gần nhất, UTC
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Thời điểm tạo media record, UTC
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Soft-delete timestamp; null khi media còn hoạt động
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<MediaObject> InverseSourceMedia { get; set; } = new List<MediaObject>();

    public virtual MediaDerivationJob? MediaDerivationJobDerivativeMedia { get; set; }

    public virtual ICollection<MediaDerivationJob> MediaDerivationJobSourceMedia { get; set; } = new List<MediaDerivationJob>();

    public virtual ICollection<MediaUsage> MediaUsages { get; set; } = new List<MediaUsage>();

    public virtual MediaObject? SourceMedia { get; set; }
}
