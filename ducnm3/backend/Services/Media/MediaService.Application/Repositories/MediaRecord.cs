// File: backend/Services/Media/MediaService.Application/Repositories/MediaRecord.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using MediaService.Application.Services.Storage;

using MediaService.Domain.Constants;

namespace MediaService.Application.Repositories;

public sealed record MediaRecord(
    Guid Id,
    StorageObjectLocation Location,
    string MediaType,
    string ContentType,
    string OriginalFileName,
    long SizeBytes,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? DeletedAtUtc,
    Guid? SourceMediaId = null,
    string? DerivationType = null,
    bool IsDraft = true,
    DateTime? DraftedAtUtc = null,
    string? ChecksumSha256 = null,
    MediaService.Domain.ValueObjects.ActorReference? UploadedBy = null);
