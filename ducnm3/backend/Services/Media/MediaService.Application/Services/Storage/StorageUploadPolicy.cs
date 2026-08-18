// File: backend/Services/Media/MediaService.Application/Services/Storage/StorageUploadPolicy.cs
// Mục đích: Định nghĩa policy/rule nghiệp vụ dùng bởi StorageUploadPolicy.

namespace MediaService.Application.Services.Storage;

public sealed record StorageUploadPolicy(
    Uri UploadUrl,
    IReadOnlyDictionary<string, string> FormFields,
    DateTime ExpiresAtUtc);
