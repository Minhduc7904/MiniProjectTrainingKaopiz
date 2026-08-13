using BuildingBlocks.Contracts.Api;

namespace MediaService.Application;

public sealed class MediaApplicationException(
    string errorCode,
    string safeMessage,
    int statusCode,
    IReadOnlyList<ApiErrorDetail>? details = null)
    : ApiException(errorCode, safeMessage, statusCode, details);
