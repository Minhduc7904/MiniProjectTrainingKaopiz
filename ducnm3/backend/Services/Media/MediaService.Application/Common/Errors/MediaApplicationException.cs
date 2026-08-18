// File: backend/Services/Media/MediaService.Application/Common/Errors/MediaApplicationException.cs
// Mục đích: Biểu diễn exception nghiệp vụ Media mang error code và HTTP status để middleware chuyển thành API envelope.

using BuildingBlocks.Contracts.Api;

namespace MediaService.Application.Common.Errors;

public sealed class MediaApplicationException(
    string errorCode,
    string safeMessage,
    int statusCode,
    IReadOnlyList<ApiErrorDetail>? details = null)
    : ApiException(errorCode, safeMessage, statusCode, details);
