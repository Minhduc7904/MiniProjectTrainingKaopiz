// File: backend/Services/Media/MediaService.Application/UseCases/MediaUsages/GetUrls/MediaUsageUrlResult.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

namespace MediaService.Application.UseCases.MediaUsages.GetUrls;

public sealed record MediaUsageUrlResult(
    Guid UsageId,
    Guid MediaId,
    string Url,
    DateTime? ExpiresAtUtc,
    uint DisplayOrder);
