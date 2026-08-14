using BuildingBlocks.Contracts.Api;

namespace NotificationService.Application;

public sealed class NotificationApplicationException(
    string errorCode,
    string safeMessage,
    int statusCode,
    IReadOnlyList<ApiErrorDetail>? details = null)
    : ApiException(errorCode, safeMessage, statusCode, details);
