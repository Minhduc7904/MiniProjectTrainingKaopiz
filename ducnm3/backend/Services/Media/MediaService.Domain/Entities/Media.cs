// File: backend/Services/Media/MediaService.Domain/Entities/Media.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using MediaService.Domain.ValueObjects;

using MediaService.Domain.Constants;

namespace MediaService.Domain.Entities;

public sealed record Media(
    Guid Id,
    string Bucket,
    string ObjectKey,
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
    ActorReference? UploadedBy = null);
