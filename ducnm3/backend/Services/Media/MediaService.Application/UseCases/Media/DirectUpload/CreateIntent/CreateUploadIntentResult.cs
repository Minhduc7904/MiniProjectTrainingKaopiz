// File: backend/Services/Media/MediaService.Application/UseCases/Media/DirectUpload/CreateIntent/CreateUploadIntentResult.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

namespace MediaService.Application.UseCases.Media.DirectUpload.CreateIntent;

public sealed record CreateUploadIntentResult(
    Guid MediaId,
    string Status,
    bool IsDraft,
    DateTime ExpiresAtUtc,
    Uri UploadUrl,
    IReadOnlyDictionary<string, string> FormFields);
