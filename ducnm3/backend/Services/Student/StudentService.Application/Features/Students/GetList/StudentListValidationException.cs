using BuildingBlocks.Contracts.Api;

namespace StudentService.Application.Features.Students.GetList;

public sealed class StudentListValidationException(
    IReadOnlyList<ApiErrorDetail> details)
    : ApiException(
        ApiErrorCodes.ValidationFailed,
        ApiErrorMessages.ValidationFailed,
        400,
        details);
