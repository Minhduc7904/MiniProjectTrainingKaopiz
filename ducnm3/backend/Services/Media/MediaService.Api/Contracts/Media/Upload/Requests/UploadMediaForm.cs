// File: backend/Services/Media/MediaService.Api/Contracts/Media/Upload/Requests/UploadMediaForm.cs
// Mục đích: Mô tả file multipart endpoint Upload Media nhận; media type được BE suy ra từ MIME.

namespace MediaService.Api.Contracts.Requests;

public sealed record UploadMediaForm(
    IFormFile File);
