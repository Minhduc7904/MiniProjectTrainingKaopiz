using System.Net.Http.Json;
using BuildingBlocks.Contracts.Api;
using CourseService.Application.Services.Media;

namespace CourseService.Infrastructure.Clients.Media;

public sealed class CourseMediaReader(HttpClient httpClient) : ICourseMediaReader
{
    public async Task<CourseMediaSet> GetAsync(
        Guid courseId,
        CancellationToken cancellationToken)
    {
        var thumbnail = await GetUsageUrlsAsync(
            "COURSE_THUMBNAIL", "THUMBNAIL", [courseId], cancellationToken);
        var gallery = await GetUsageUrlsAsync(
            "COURSE_GALLERY", "ATTACHMENT", [courseId], cancellationToken);
        return new CourseMediaSet(thumbnail.Count == 0 ? null : thumbnail[0], gallery);
    }

    public async Task<IReadOnlyDictionary<Guid, CourseMediaSet>> GetManyAsync(
        IReadOnlyList<Guid> courseIds,
        CancellationToken cancellationToken)
    {
        if (courseIds.Count == 0)
        {
            return new Dictionary<Guid, CourseMediaSet>();
        }

        var thumbnailTask = GetUsageUrlsAsync("COURSE_THUMBNAIL", "THUMBNAIL", courseIds, cancellationToken);
        var galleryTask = GetUsageUrlsAsync("COURSE_GALLERY", "ATTACHMENT", courseIds, cancellationToken);
        await Task.WhenAll(thumbnailTask, galleryTask);
        var thumbnails = thumbnailTask.Result
            .GroupBy(item => item.OwnerId)
            .ToDictionary(group => group.Key, group => group.First());
        var galleries = galleryTask.Result
            .GroupBy(item => item.OwnerId)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<CourseMediaAsset>)group.ToArray());
        return courseIds.Distinct().ToDictionary(
            courseId => courseId,
            courseId => new CourseMediaSet(
                thumbnails.GetValueOrDefault(courseId),
                galleries.GetValueOrDefault(courseId, [])));
    }

    public Task<IReadOnlyList<CourseMediaAsset>> GetLessonAttachmentsAsync(
        Guid lessonId,
        CancellationToken cancellationToken) =>
        GetUsageUrlsAsync("LESSON_ATTACHMENT", "ATTACHMENT", [lessonId], cancellationToken);

    private async Task<IReadOnlyList<CourseMediaAsset>> GetUsageUrlsAsync(
        string ownerType,
        string usageType,
        IReadOnlyList<Guid> courseIds,
        CancellationToken cancellationToken)
    {
        var ownerIds = string.Join(',', courseIds.Select(courseId => courseId.ToString("D")));
        var path = $"{ApiRoutes.Media.UsageUrls}?ownerService=COURSE&ownerType={ownerType}&usageType={usageType}&ownerIds={ownerIds}";
        var envelope = await httpClient.GetFromJsonAsync<ApiResponse<IReadOnlyList<MediaUsageUrlResponse>>>(path, cancellationToken)
            ?? throw new InvalidOperationException("Media Service returned an empty response.");
        return envelope.Data?.Select(item => new CourseMediaAsset(
            item.UsageId,
            item.MediaId,
            item.OwnerId,
            item.ContentUrl,
            item.ThumbnailUrl,
            item.ExpiresAtUtc,
            item.DisplayOrder,
            item.MediaType,
            item.ContentType,
            item.OriginalFileName)).ToArray() ?? [];
    }

    private sealed record MediaUsageUrlResponse(
        Guid UsageId,
        Guid MediaId,
        Guid OwnerId,
        string ContentUrl,
        string? ThumbnailUrl,
        DateTime? ExpiresAtUtc,
        uint DisplayOrder,
        string MediaType,
        string ContentType,
        string OriginalFileName);
}
