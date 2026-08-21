// File: backend/Services/Media/MediaService.Application/UseCases/MediaUsageJobs/GetList/GetMediaBackgroundJobsResult.cs
// Mục đích: Cung cấp kết quả list đã tính metadata pagination cho API.

using MediaService.Application.Repositories;

namespace MediaService.Application.UseCases.MediaUsageJobs.GetList;

public sealed record GetMediaBackgroundJobsResult(
    IReadOnlyList<MediaBackgroundJobListRecord> Items,
    int Page,
    int PageSize,
    long TotalItems,
    int TotalPages);
