// File: backend/Services/Media/MediaService.Application/UseCases/MediaUsageJobs/GetList/MediaBackgroundJobListPage.cs
// Mục đích: Trả danh sách job và metadata offset pagination từ repository.

using MediaService.Application.Repositories;

namespace MediaService.Application.UseCases.MediaUsageJobs.GetList;

public sealed record MediaBackgroundJobListPage(
    IReadOnlyList<MediaBackgroundJobListRecord> Items,
    long TotalItems);
