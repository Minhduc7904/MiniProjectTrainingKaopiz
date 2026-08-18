// File: backend/Services/Media/MediaService.Api/Contracts/Media/Upload/Requests/UploadMediaForm.cs
// Mục đích: Mô tả các trường multipart form endpoint Upload Media nhận, gồm file, owner và metadata đi kèm.

namespace MediaService.Api.Contracts.Requests;

public sealed record UploadMediaForm(
    IFormFile File,
    string MediaType,
    string UploadedByType,
    string UploadedBy);
