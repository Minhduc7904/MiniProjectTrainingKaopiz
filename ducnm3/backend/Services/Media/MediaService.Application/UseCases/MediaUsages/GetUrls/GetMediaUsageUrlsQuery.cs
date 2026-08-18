// File: backend/Services/Media/MediaService.Application/UseCases/MediaUsages/GetUrls/GetMediaUsageUrlsQuery.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

namespace MediaService.Application.UseCases.MediaUsages.GetUrls;

public sealed record GetMediaUsageUrlsQuery(
    string OwnerService,
    string OwnerType,
    string UsageType,
    Guid OwnerId);
