// File: backend/Services/Media/MediaService.Application/UseCases/MediaUsageJobs/GetList/GetMediaBackgroundJobsQuery.cs
// Mục đích: Nhận query thô từ transport cho API liệt kê Media background job.

using MediaService.Domain.ValueObjects;

namespace MediaService.Application.UseCases.MediaUsageJobs.GetList;

public sealed record GetMediaBackgroundJobsQuery(
    string? JobType,
    string? Status,
    string? CorrelationId,
    int Page,
    int PageSize,
    ActorReference Actor);
