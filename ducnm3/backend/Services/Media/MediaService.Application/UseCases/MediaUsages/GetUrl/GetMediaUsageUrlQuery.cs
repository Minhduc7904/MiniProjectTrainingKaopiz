// File: backend/Services/Media/MediaService.Application/UseCases/MediaUsages/GetUrl/GetMediaUsageUrlQuery.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

namespace MediaService.Application.UseCases.MediaUsages.GetUrl;

public sealed record GetMediaUsageUrlQuery(Guid UsageId);
