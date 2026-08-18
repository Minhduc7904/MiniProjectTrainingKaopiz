// File: backend/Services/Media/MediaService.Api/Contracts/MediaDerivations/RetryThumbnail/RetryMediaThumbnailRequest.cs
// Mục đích: Định nghĩa request contract HTTP cho RetryMediaThumbnailRequest.

namespace MediaService.Api.Contracts.Requests;

public sealed record RetryMediaThumbnailRequest(
    string RequestedBy,
    string RequestedByType);
