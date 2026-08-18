// File: backend/Services/Media/MediaService.Api/Contracts/Media/DirectUpload/CreateIntent/CreateUploadIntentResponse.cs
// Mục đích: Định nghĩa contract chia sẻ của Media Service.

namespace MediaService.Api.Contracts.Responses;

public sealed record CreateUploadIntentResponse(
    Guid MediaId,
    string Status,
    bool IsDraft,
    DateTime ExpiresAtUtc,
    Uri UploadUrl,
    IReadOnlyDictionary<string, string> FormFields);
