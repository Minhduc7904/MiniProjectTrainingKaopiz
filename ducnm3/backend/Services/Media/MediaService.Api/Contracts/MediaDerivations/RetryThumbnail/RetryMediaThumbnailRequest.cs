// File: backend/Services/Media/MediaService.Api/Contracts/MediaDerivations/RetryThumbnail/RetryMediaThumbnailRequest.cs
// Mục đích: Định nghĩa contract chia sẻ của Media Service.

namespace MediaService.Api.Contracts.Requests;

public sealed record RetryMediaThumbnailRequest(
    string RequestedBy,
    string RequestedByType);
