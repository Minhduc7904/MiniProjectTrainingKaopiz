// File: backend/Services/Media/MediaService.Application/Services/Storage/StorageUploadPolicyRequest.cs
// Mục đích: Định nghĩa request contract HTTP cho StorageUploadPolicyRequest.

namespace MediaService.Application.Services.Storage;

public sealed record StorageUploadPolicyRequest(
    StorageObjectLocation Location,
    string ContentType,
    long SizeBytes,
    string ChecksumSha256);
