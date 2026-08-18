// File: backend/Services/Media/MediaService.Api/Contracts/Media/DirectUpload/CreateIntent/CreateUploadIntentRequest.cs
// Mục đích: Định nghĩa request contract HTTP cho CreateUploadIntentRequest.

namespace MediaService.Api.Contracts.Requests;

public sealed record CreateUploadIntentRequest(
    string OriginalFileName,
    string MediaType,
    string ContentType,
    long SizeBytes,
    string ChecksumSha256,
    string UploadedBy,
    string UploadedByType);
