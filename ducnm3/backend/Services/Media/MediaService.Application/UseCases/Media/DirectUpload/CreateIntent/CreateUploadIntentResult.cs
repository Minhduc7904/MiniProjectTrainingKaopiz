// File: backend/Services/Media/MediaService.Application/UseCases/Media/DirectUpload/CreateIntent/CreateUploadIntentResult.cs
// Mục đích: Định nghĩa dữ liệu đầu ra của use case CreateUploadIntentResult.

namespace MediaService.Application.UseCases.Media.DirectUpload.CreateIntent;

public sealed record CreateUploadIntentResult(
    Guid MediaId,
    string Status,
    bool IsDraft,
    DateTime ExpiresAtUtc,
    Uri UploadUrl,
    IReadOnlyDictionary<string, string> FormFields);
