// File: backend/Services/Media/MediaService.Application/UseCases/MediaUsages/GetUrls/GetMediaUsageUrlsQuery.cs
// Mục đích: Định nghĩa dữ liệu truy vấn cho use case GetMediaUsageUrlsQuery.

namespace MediaService.Application.UseCases.MediaUsages.GetUrls;

public sealed record GetMediaUsageUrlsQuery(
    string OwnerService,
    string OwnerType,
    string UsageType,
    Guid OwnerId);
