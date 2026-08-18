// File: backend/Services/Media/MediaService.Application/UseCases/MediaUsages/GetUrl/GetMediaUsageUrlQuery.cs
// Mục đích: Định nghĩa dữ liệu truy vấn cho use case GetMediaUsageUrlQuery.

namespace MediaService.Application.UseCases.MediaUsages.GetUrl;

public sealed record GetMediaUsageUrlQuery(Guid UsageId);
