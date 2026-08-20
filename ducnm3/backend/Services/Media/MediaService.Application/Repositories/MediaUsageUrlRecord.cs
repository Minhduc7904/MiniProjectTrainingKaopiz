// File: backend/Services/Media/MediaService.Application/Repositories/MediaUsageUrlRecord.cs
// Mục đích: Mô tả dữ liệu MediaUsageUrlRecord được repository đọc hoặc ghi giữa Application và Persistence.

namespace MediaService.Application.Repositories;

public sealed record MediaUsageUrlRecord(
    MediaUsageRecord Usage,
    MediaRecord Media,
    MediaRecord? ThumbnailMedia = null);
