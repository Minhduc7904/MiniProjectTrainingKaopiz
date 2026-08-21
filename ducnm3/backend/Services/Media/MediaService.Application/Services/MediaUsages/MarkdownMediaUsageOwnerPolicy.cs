// File: backend/Services/Media/MediaService.Application/Services/MediaUsages/MarkdownMediaUsageOwnerPolicy.cs
// Mục đích: Whitelist owner được phép đồng bộ media tham chiếu trong Markdown.

using MediaService.Application.Common.Errors;
using MediaService.Contracts.Messaging;
using MediaService.Domain.Constants;

namespace MediaService.Application.Services.MediaUsages;

public static class MarkdownMediaUsageOwnerPolicy
{
    public static void Validate(string ownerService, string ownerType)
    {
        if (ownerService != MediaOwnerServices.Course ||
            ownerType is not (MediaOwnerTypes.CourseDescription or MediaOwnerTypes.LessonContent))
        {
            throw MediaErrors.InvalidMedia("The Markdown media usage owner is not supported.");
        }
    }

    public static void ValidateReference(MarkdownMediaUsageReferenceV1 reference)
    {
        if (reference.MediaId == Guid.Empty ||
            reference.UsageType is not (MarkdownMediaUsageTypes.Embed or MarkdownMediaUsageTypes.Attachment))
        {
            throw MediaErrors.InvalidMedia("The Markdown media usage reference is invalid.");
        }
    }
}
