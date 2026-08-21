using BuildingBlocks.Contracts.Api;

namespace StudentService.Application.Common.Errors;

public sealed class StudentApplicationException(
    string errorCode,
    string safeMessage,
    int statusCode,
    IReadOnlyList<ApiErrorDetail>? details = null)
    : ApiException(errorCode, safeMessage, statusCode, details);
