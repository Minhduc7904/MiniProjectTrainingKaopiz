// File: backend/Services/Course/CourseService.Application/UseCases/LessonAi/Documents/LessonAiDocumentView.cs
// Mục đích: Hợp nhất tệp đính kèm (từ Media Service) với trạng thái trích xuất (từ database Course) thành danh sách hiển thị cho admin.

using CourseService.Application.Repositories;
using CourseService.Application.Services.Media;
using CourseService.Domain.Constants;

namespace CourseService.Application.UseCases.LessonAi.Documents;

public sealed record LessonAiDocumentView(
    Guid MediaId,
    string FileName,
    string ContentType,
    string MediaType,
    string Status,
    string? ErrorCode,
    string? ErrorMessage,
    int TextLength,
    DateTime? UpdatedAtUtc);

internal static class LessonAiDocumentViewBuilder
{
    public static IReadOnlyList<LessonAiDocumentView> Build(
        IReadOnlyList<CourseMediaAsset> attachments,
        IReadOnlyList<LessonAiDocumentRecord> records)
    {
        var byMedia = records.ToDictionary(record => record.MediaId);
        return attachments
            .OrderBy(attachment => attachment.DisplayOrder)
            .Select(attachment =>
            {
                if (byMedia.TryGetValue(attachment.MediaId, out var record))
                {
                    return new LessonAiDocumentView(
                        attachment.MediaId, attachment.OriginalFileName, attachment.ContentType, attachment.MediaType,
                        record.Status, record.ErrorCode, record.ErrorMessage, record.TextLength, record.UpdatedAtUtc);
                }

                var unsupported = LessonAiDocumentKinds.Classify(attachment.ContentType, attachment.OriginalFileName) == LessonAiDocumentKind.Unsupported;
                return new LessonAiDocumentView(
                    attachment.MediaId, attachment.OriginalFileName, attachment.ContentType, attachment.MediaType,
                    unsupported ? LessonAiDocumentStatuses.Unsupported : LessonAiDocumentStatuses.NotQueued,
                    unsupported ? LessonAiDocumentKinds.UnsupportedErrorCode : null,
                    unsupported ? LessonAiDocumentKinds.UnsupportedMessage : null,
                    0,
                    null);
            })
            .ToArray();
    }
}
