// File: backend/Services/Media/MediaService.Application/Services/Storage/StorageUploadPolicy.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

namespace MediaService.Application.Services.Storage;

public sealed record StorageUploadPolicy(
    Uri UploadUrl,
    IReadOnlyDictionary<string, string> FormFields,
    DateTime ExpiresAtUtc);
