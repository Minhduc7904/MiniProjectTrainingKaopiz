using BuildingBlocks.Contracts.Api;

namespace StudentService.Application.Common.Errors;

public static class StudentErrors
{
    public static StudentApplicationException ValidationFailed(
        IReadOnlyList<ApiErrorDetail>? details = null) =>
        new(ApiErrorCodes.ValidationFailed, ApiErrorMessages.ValidationFailed, 400, details);

    public static StudentApplicationException NotFound() =>
        new(StudentErrorCodes.NotFound, "Student not found.", 404);

    public static StudentApplicationException EmailExists() =>
        new(StudentErrorCodes.EmailExists, "A student with this email already exists.", 409);

    public static StudentApplicationException NotActive() =>
        new(StudentErrorCodes.NotActive, "Student is not active.", 403);
}
