// File: backend/Services/Media/MediaService.Application/UseCases/MediaUsageJobs/GetList/MediaBackgroundJobListRequest.cs
// Mục đích: Biểu diễn query đã validate và normalize trước khi Infrastructure truy vấn database.

namespace MediaService.Application.UseCases.MediaUsageJobs.GetList;

public sealed record MediaBackgroundJobListRequest(
    string? JobType,
    string? Status,
    Guid? CorrelationId,
    int Page,
    int PageSize);
