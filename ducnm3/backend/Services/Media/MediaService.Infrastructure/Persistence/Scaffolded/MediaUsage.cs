// File: backend/Services/Media/MediaService.Infrastructure/Persistence/Scaffolded/MediaUsage.cs
// Mục đích: Entity EF scaffolded MediaUsage ánh xạ bảng database hiện có; phục vụ persistence và không chứa nghiệp vụ use case.

using System;
using System.Collections.Generic;

namespace MediaService.Infrastructure.Persistence.Scaffolded;

public partial class MediaUsage
{
    /// <summary>
    /// UUID định danh liên kết usage
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// UUID media_objects.id trong Media Service database
    /// </summary>
    public Guid MediaId { get; set; }

    /// <summary>
    /// COURSE | NOTIFICATION
    /// </summary>
    public string OwnerService { get; set; } = null!;

    /// <summary>
    /// COURSE_THUMBNAIL | COURSE_GALLERY | COURSE_DESCRIPTION | LESSON_CONTENT | NOTIFICATION_BODY
    /// </summary>
    public string OwnerType { get; set; } = null!;

    /// <summary>
    /// UUID owner ở owner_service; logical reference
    /// </summary>
    public Guid OwnerId { get; set; }

    /// <summary>
    /// THUMBNAIL | EMBED | ATTACHMENT
    /// </summary>
    public string UsageType { get; set; } = null!;

    /// <summary>
    /// Thứ tự render media trong cùng một owner
    /// </summary>
    public uint DisplayOrder { get; set; }

    /// <summary>
    /// UUID user/admin tạo liên kết; logical reference
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Actor type tạo usage; được Application validate
    /// </summary>
    public string CreatedByType { get; set; } = null!;

    /// <summary>
    /// Thời điểm tạo liên kết, UTC
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Soft-delete timestamp; null khi usage còn hiệu lực
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Owner Course có thumbnail còn hiệu lực; dùng để đảm bảo tối đa một thumbnail
    /// </summary>
    public Guid? ActiveCourseThumbnailOwnerId { get; set; }

    /// <summary>
    /// Student có avatar active; đảm bảo tối đa một avatar
    /// </summary>
    public Guid? ActiveStudentAvatarOwnerId { get; set; }

    /// <summary>
    /// Media gốc có thumbnail active; đảm bảo tối đa một thumbnail
    /// </summary>
    public Guid? ActiveMediaThumbnailOwnerId { get; set; }

    /// <summary>
    /// Chỉ áp dụng unique reference cho usage active
    /// </summary>
    public sbyte? ActiveReferenceGuard { get; set; }

    public virtual MediaObject Media { get; set; } = null!;
}
