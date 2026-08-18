// File: backend/Services/Media/MediaService.Api/Contracts/Media/Upload/Requests/UploadMediaForm.cs
// Mục đích: Định nghĩa contract chia sẻ của Media Service.

namespace MediaService.Api.Contracts.Requests;

public sealed record UploadMediaForm(
    IFormFile File,
    string MediaType,
    string UploadedByType,
    string UploadedBy);
