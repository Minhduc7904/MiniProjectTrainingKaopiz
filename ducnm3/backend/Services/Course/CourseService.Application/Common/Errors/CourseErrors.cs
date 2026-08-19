// File: backend/Services/Course/CourseService.Application/Common/Errors/CourseErrors.cs
// Mục đích: Tập trung tạo lỗi Course an toàn, nhất quán với Common Errors của Media Service.

using BuildingBlocks.Contracts.Api;

namespace CourseService.Application.Common.Errors;

public static class CourseErrors
{
    public static CourseApplicationException ValidationFailed(
        IReadOnlyList<ApiErrorDetail> details) =>
        new(
            ApiErrorCodes.ValidationFailed,
            ApiErrorMessages.ValidationFailed,
            400,
            details);

    public static CourseApplicationException CourseNotFound() =>
        new(CourseErrorCodes.CourseNotFound, "Course was not found.", 404);
}
