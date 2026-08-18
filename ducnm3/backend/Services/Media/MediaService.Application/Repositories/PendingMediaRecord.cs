// File: backend/Services/Media/MediaService.Application/Repositories/PendingMediaRecord.cs
// Mục đích: Mô tả dữ liệu PendingMediaRecord được repository đọc hoặc ghi giữa Application và Persistence.

using MediaService.Application.Services.Storage;
using MediaService.Domain.ValueObjects;

using MediaService.Domain.Constants;

namespace MediaService.Application.Repositories;

public sealed record PendingMediaRecord(
    Guid Id,
    StorageObjectLocation Location,
    string MediaType,
    string ContentType,
    string OriginalFileName,
    long SizeBytes,
    ActorReference UploadedBy,
    bool IsDraft,
    DateTime? DraftedAtUtc,
    string? ExpectedChecksumSha256 = null);
