// File: backend/Services/Media/MediaService.Api/Contracts/Media/DirectUpload/Complete/CompleteDirectUploadRequest.cs
// Mục đích: Định nghĩa contract chia sẻ của Media Service.

namespace MediaService.Api.Contracts.Requests;

public sealed record CompleteDirectUploadRequest(
    string UploadedBy,
    string UploadedByType);
