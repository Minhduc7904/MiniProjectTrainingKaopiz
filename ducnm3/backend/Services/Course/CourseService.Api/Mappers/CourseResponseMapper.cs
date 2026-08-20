// File: backend/Services/Course/CourseService.Api/Mappers/CourseResponseMapper.cs
// Mục đích: Chuyển dữ liệu use case Course thành HTTP contract công khai.

using CourseService.Api.Contracts.Courses;
using CourseService.Application.Repositories;
using CourseService.Application.Services.Media;
using CourseService.Application.UseCases.Courses.GetDetails;

namespace CourseService.Api.Mappers;

public static class CourseResponseMapper
{
    public static IReadOnlyList<CourseListItemResponse> ToListResponse(
        IReadOnlyList<CourseListItemRecord> items,
        IReadOnlyDictionary<Guid, string?> thumbnails) =>
        items.Select(item => new CourseListItemResponse(
            item.Id, item.Name, item.Status, item.CreatedAtUtc,
            thumbnails.GetValueOrDefault(item.Id))).ToArray();

    public static CourseDetailsResponse ToDetailsResponse(
        CourseDetailsResult result,
        CourseMediaSet media) =>
        new(
            result.Id,
            result.Name,
            result.DescriptionMarkdown,
            result.Status,
            result.CreatedAtUtc,
            ToOptionalMediaResponse(media.Thumbnail),
            media.Gallery.Select(ToMediaResponse).ToArray(),
            result.Lessons.Select(lesson => new LessonDetailsResponse(
                lesson.Id,
                lesson.Title,
                lesson.ContentMarkdown,
                lesson.DisplayOrder,
                lesson.Progresses.Select(progress => new LessonProgressResponse(
                    progress.StudentId,
                    progress.ProgressPercent,
                    progress.CompletedAtUtc,
                    progress.UpdatedAtUtc)).ToArray())).ToArray());

    public static CourseMediaResponse ToMediaResponse(CourseMediaAsset media) =>
        new(
            media.UsageId,
            media.MediaId,
            media.ContentUrl,
            media.ThumbnailUrl,
            media.ExpiresAtUtc,
            media.DisplayOrder,
            media.MediaType,
            media.ContentType,
            media.OriginalFileName);

    private static CourseMediaResponse? ToOptionalMediaResponse(CourseMediaAsset? media) =>
        media is null ? null : ToMediaResponse(media);
}
