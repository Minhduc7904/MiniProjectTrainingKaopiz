// File: backend/Services/Media/MediaService.Contracts/Messaging/SynchronizeMarkdownMediaUsageV1.cs
// Mục đích: Contract đồng bộ usage của media được tham chiếu trong Markdown theo owner tổng quát.

using BuildingBlocks.Messaging.Abstractions;

namespace MediaService.Contracts.Messaging;

public sealed record SynchronizeMarkdownMediaUsageV1(
    string OwnerService,
    string OwnerType,
    Guid OwnerId,
    Guid CreatedBy,
    IReadOnlyList<MarkdownMediaUsageReferenceV1> Added,
    IReadOnlyList<MarkdownMediaUsageReferenceV1> Removed) : ICommand;

public sealed record MarkdownMediaUsageReferenceV1(
    Guid MediaId,
    string UsageType,
    uint DisplayOrder);

public static class MarkdownMediaUsageOwnerServices
{
    public const string Course = "COURSE";
}

public static class MarkdownMediaUsageOwnerTypes
{
    public const string CourseThumbnail = "COURSE_THUMBNAIL";
    public const string CourseGallery = "COURSE_GALLERY";
    public const string CourseDescription = "COURSE_DESCRIPTION";
    public const string LessonContent = "LESSON_CONTENT";
    public const string LessonAttachment = "LESSON_ATTACHMENT";
}

public static class MarkdownMediaUsageTypes
{
    public const string Attachment = "ATTACHMENT";
    public const string Embed = "EMBED";
}
