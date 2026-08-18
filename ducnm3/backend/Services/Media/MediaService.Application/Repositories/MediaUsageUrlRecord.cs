// File: backend/Services/Media/MediaService.Application/Repositories/MediaUsageUrlRecord.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

namespace MediaService.Application.Repositories;

public sealed record MediaUsageUrlRecord(
    MediaUsageRecord Usage,
    MediaRecord Media);
