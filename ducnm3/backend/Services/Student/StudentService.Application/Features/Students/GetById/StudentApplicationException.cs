using BuildingBlocks.Contracts.Api;

namespace StudentService.Application.Features.Students.GetById;

public sealed class StudentApplicationException(
    string errorCode,
    string safeMessage,
    int statusCode)
    : ApiException(errorCode, safeMessage, statusCode);
