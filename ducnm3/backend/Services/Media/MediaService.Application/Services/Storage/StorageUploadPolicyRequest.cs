// File: backend/Services/Media/MediaService.Application/Services/Storage/StorageUploadPolicyRequest.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

namespace MediaService.Application.Services.Storage;

public sealed record StorageUploadPolicyRequest(
    StorageObjectLocation Location,
    string ContentType,
    long SizeBytes,
    string ChecksumSha256);
