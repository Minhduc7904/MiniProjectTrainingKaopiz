// File: backend/Services/Media/MediaService.Application/Common/Errors/MediaApplicationException.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using BuildingBlocks.Contracts.Api;

namespace MediaService.Application.Common.Errors;

public sealed class MediaApplicationException(
    string errorCode,
    string safeMessage,
    int statusCode,
    IReadOnlyList<ApiErrorDetail>? details = null)
    : ApiException(errorCode, safeMessage, statusCode, details);
